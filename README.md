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