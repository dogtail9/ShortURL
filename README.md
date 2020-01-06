# A ASP.NET Core application hosted on different plattforms

This project is a tutorial on how to host an ASP.NET Core application on Kubernetes, Docker Swarm or Azure App Services.
The appllication is a short url application. The application stores long urls behind a shorter key value. 
When the user browse to the key value the browser will be redirected to the long url.

## Prerequisites

* Docker for Windows
* Visual Studio 2019

## The Application

The application consists of two parts, one service for redirection and one service for adding new urls.
We will also write a Gui application for adding urls.
The data will be stored in SQL Server and cached in Redis nere the redirect service. 
The cache will be updated using RabbitMQ when a url is added, deleted or updated.
The application will trace every call using Open Telemetry and W3C Trace Context. 
Zipkin will be used to gather the logs.

### Implement the Redirect API

Start by creating a ASP.NET Core API project.

Add a folder named Data.
Add an interface named `IUrlRepository`.

```c#
using System.Threading.Tasks;

namespace ShortUrl.RedirectApi.DataAccess
{
    public interface IUrlRepository
    {
        Task<string> GetUrlAsync(string key);
    }
}

```

Let's start with an inmemory implementation of the `IUrlRepository` interface.

```c#
using System;
using System.Threading.Tasks;

namespace ShortUrl.RedirectApi.DataAccess
{
    public class InMemoryUrlRepository : IUrlRepository
    {
        private const string KeyNotFound = "Key not found.";

        public async Task<string> GetUrlAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == "ms")
                return "http://www.microsoft.com";

            if (key == "ibm")
                return "http://www.ibm.com";

            throw new ArgumentException(KeyNotFound);
        }
    }
}
```

Implement the `RedirectController`. The `IUrlRepository` is injected and used to find the url for the key.
If no key is found the Api will return a `404 Not Found`.
There is also a special key *info*, Info will return a View insted of redirecting the user to an url. We will use this feature to se if load balansing works for the redirect service.

```c#
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortUrl.RedirectApi.DataAccess;

namespace ShortUrl.RedirectApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedirectController : Controller
    {
        private readonly IUrlRepository _repository;
        private readonly ILogger<RedirectController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RedirectController(IUrlRepository repository, ILogger<RedirectController> logger, IWebHostEnvironment hostEnvironment)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> RedirectTo(string key)
        {
            if (_hostEnvironment.IsDevelopment() && key == "info")
                return View("Info");

            try
            {
                var url = await _repository.GetUrlAsync(key);

                return Redirect(url);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
```

Create a folder named `Views`. In the views folder create a `Redirect` folder.
Add a view named `info`. The info view displays the name of the machine the api service is hosted on.

```html
<!DOCTYPE html>

<html>
<head>
    <title>Node Info</title>
</head>
<body>
    <h1>Machine Name: @Environment.MachineName</h1>
</body>
</html>
```

Change the `launchSettings.json` file to browse to `redirect/info` when debugging the application.

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:56887",
      "sslPort": 0
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "redirect/info",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "ShortUrl.RedirectApi": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "redirect/info",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Run the application. Browse to [https://localhost:5001/redirect/info](https://localhost:5001/redirect/info) and the name of your host will be displayed.
Browse to [https://localhost:5000/redirect/ms](https://localhost:5000/redirect/ms) and the browser will be redirected to microsoft.com.

### Implement the Url Management API

Create another ASP.NET Core API project.

Add a folder named DataAccess.
Add a class named `ShortUrlModel`.

```c#
using System.ComponentModel.DataAnnotations;

namespace ShortUrl.UrlManagementApi.DataAccess
{
    public class ShortUrlModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Key { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Url { get; set; }
    }
}
```

Add another class named `UrlDbContext`.

```c#
using Microsoft.EntityFrameworkCore;

namespace ShortUrl.UrlManagementApi.DataAccess
{
    public class UrlDbContext : DbContext
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShortUrlModel> ShortUrl { get; set; }
    }
}
```

Implement the `ManagementController`.

```c#
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShortUrl.UrlManagementApi.DataAccess;

namespace ShortUrl.UrlManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManagementController : ControllerBase
    {
        private readonly UrlDbContext _context;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(UrlDbContext context, ILogger<ManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shortUrlModels = await _context.ShortUrl.ToListAsync();

            return new JsonResult(shortUrlModels);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long? id)
        {
            var shortUrlModel = await _context.ShortUrl.FindAsync(id);

            if (shortUrlModel is null)
            {
                return NotFound();
            }

            return new JsonResult(shortUrlModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShortUrlModel shortUrlModel)
        {
            _context.Add(shortUrlModel);
            await _context.SaveChangesAsync();

            return Created(shortUrlModel.Key, shortUrlModel);
        }

        [HttpDelete]
        [Route("{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id is null)
                return NotFound();

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Id == id);

            if (shortUrlModel is null)
                return NotFound();

            _context.ShortUrl.Remove(shortUrlModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
```

Add the `UrlDbContext` to IoC.
Run migrate on application startup, this is a bad idé to do in production thats why it is only done in the development environment.

```c#
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortUrl.UrlManagementApi.DataAccess;

namespace ShortUrl.UrlManagementApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<UrlDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("UrlDbContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                UpdateDatabase(app);
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<UrlDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
```

Add the connection string to the database in `appsettings.Development.json`.
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "UrlDbContext": "Server=localhost;Database=ShortUrl;User Id=sa;Password=P@ssw0rd;"
  }
}
```

Start the SQL Server container.

```powershell
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04
```

Add a migration step to create the database by runing the following command in the Package Manager Console in Visual Studio.

```powershell
Add-Migration InitialCreate 
```

Change the `launchSettings.json` file to browse to `Management` when debugging the application.

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:56990",
      "sslPort": 0
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "Management",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "ShortUrl.UrlManagementApi": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "Management",
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Run the application. Browse to [https://localhost:5001/Management](https://localhost:5001/Management), an empty json array will be returned from the api.

To add an url to the database `POST` `{"key":"a","url":"http://www.microsoft.com"}` to [https://localhost:5001/Management](https://localhost:5001/Management). I am using Postman to do this.
Refresh the browser and now you will see that the json array contains one item.

```json
[
  {
    "id":1,
    "key":"a",
    "url":"http://www.microsoft.com"
  }
]
```

Browse to [https://localhost:5001/Management/1](https://localhost:5001/Management/1).
The specific entry will be returned.

```json
{
  "id":1,
  "key":"a",
  "url":"http://www.microsoft.com"
}
```

To delete an entry in the database send a `DELETE` to [https://localhost:5001/Management/1](https://localhost:5001/Management/1).
This will delete the entry and the database will be empty again.

### Refactor data access

It is now possible to get, add and delete urls to the datadase through the management api service but the redirect service has an in memory implementation of the `IUrlRepository` interface to get the data.
Let's move the data access code to a new project and implement a `SqlUrlRepository` that will be used by both the management and the redirect services.

Create a new `ClassLibrary` project named `ShortUrl.DataAccess.Sql`.
Add references from both the management and the redirect project to the data access project.
Move the `IUrlRepository`, `ShortUrlModel` and the `UrlDbContext` types to the `ShortUrl.DataAccess.Sql` project.

Create a new class named `SqlServerDesignTimeDbContextFactory` and implement the `IDesignTimeDbContextFactory` interface.

```c#
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShortUrl.DataAccess.Sql
{
    public class SqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<UrlDbContext>
    {
        public UrlDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server = localhost; Database = ShortUrl; User Id = sa; Password = P@ssw0rd;";
            var optionsBuilder = new DbContextOptionsBuilder<UrlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new UrlDbContext(optionsBuilder.Options);
        }
    }
}
```

Run the migration again against the `ShortUrl.DataAccess.Sql` project.

```powershell
Add-Migration CreateDatabase
```

Delete the `Migrations` folder in the `ShortUrl.UrlManagementApi` project.

Add methods to get all, add and delete entities in the `IUrlRepository` interface.

```c#
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.DataAccess.Sql
{
    public interface IUrlRepository
    {
        Task<IEnumerable<ShortUrlModel>> GetAll();

        Task<string> GetUrlAsync(string key);

        Task<string> GetUrlAsync(long? id);

        Task<ShortUrlModel> AddUrl(string key, string url);

        Task DeleteUrl(long? id);
    }
}
```

Create a new class named `SqlUrlRepository` and implement the `IUrlRepository` interface.

```c#
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.DataAccess.Sql
{
    public class SqlUrlRepository : IUrlRepository
    {
        private const string KeyNotFound = "Key not found.";
        private const string AgumentNeedsToHaveAValue = "Argument needs to have a value.";

        private readonly UrlDbContext _context;

        public SqlUrlRepository(UrlDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ShortUrlModel>> GetAll()
        {
            var shortUrlModels = await _context.ShortUrl.ToListAsync();

            return shortUrlModels;
        }

        public async Task<ShortUrlModel> AddUrl(string key, string url)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(AgumentNeedsToHaveAValue, nameof(key));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException(AgumentNeedsToHaveAValue, nameof(url));

            var shortUrlModel = new ShortUrlModel { Key = key, Url = url };
            _context.Add(shortUrlModel);
            await _context.SaveChangesAsync();

            return shortUrlModel;
        }

        public async Task DeleteUrl(long? id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Id == id);
            
            if (shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            _context.ShortUrl.Remove(shortUrlModel);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetUrlAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Key == key);

            if(shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            return shortUrlModel.Url;
        }

        public async Task<string> GetUrlAsync(long? id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Id == id);

            if (shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            return shortUrlModel.Url;
        }
    }
}
```

Replace the `InMemoryUrlRepository` with the newly created `SqlUrlRepository` in the `ShortUrl.RedirectApi` project.
In the `Startup` class add the `UrlDbContext` and `SqlUrlRepository` to the IoC.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddDbContext<UrlDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("UrlDbContext")));
    services.AddScoped<IUrlRepository, SqlUrlRepository>();
}
```

Add the connectionsting to the database in the `appsettings.Development.json` file.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "UrlDbContext": "Server=localhost;Database=ShortUrl;User Id=sa;Password=P@ssw0rd;"
  }
}
```

Change the `ConfigureServices` metod int the `Satrtup` class in the `ShortUrl.UrlManagementApi` projrct.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddDbContext<UrlDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("UrlDbContext")));
    services.AddScoped<IUrlRepository, SqlUrlRepository>();
}
```

Replace the `UrlDbContext` with the `IUrlRepository` in the `ManagementController` class.

```c#
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShortUrl.DataAccess.Sql;

namespace ShortUrl.UrlManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManagementController : ControllerBase
    {
        private readonly IUrlRepository _repository;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(IUrlRepository repository, ILogger<ManagementController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shortUrlModels = await _repository.GetAll();

            return new JsonResult(shortUrlModels);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long? id)
        {
            try
            {
                var shortUrlModel = await _repository.GetUrlAsync(id);

                return new JsonResult(shortUrlModel);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShortUrlModel shortUrlModel)
        {
            try
            {
                await _repository.AddUrl(shortUrlModel.Key, shortUrlModel.Url);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }

            return Created(shortUrlModel.Key, shortUrlModel);
        }

        [HttpDelete]
        [Route("{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id is null)
                return NotFound();

            try
            {
                await _repository.DeleteUrl(id);

                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
```

Set both the `ShortUrl.RedirectApi` and the `ShortUrl.UrlManagementApi` project as startup projects and run the application.
It is now possible to add an url bia the managemant service and browse to that key en be redirected to the coresponding url in the redirect service.

### Add Swagger to the service projects.

Before creating the gui application to manage the urls it it a good idé to add Swagger to the services so we can generate client code.

