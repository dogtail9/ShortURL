using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortUrl.DataAccess.Sql;

namespace ShortUrl.RedirectApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedirectController : Controller
    {
        private readonly IUrlRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedirectController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RedirectController(IUrlRepository repository, IDistributedCache cache, ILogger<RedirectController> logger, IWebHostEnvironment hostEnvironment)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> RedirectTo(string key)
        {
            if (_hostEnvironment.IsDevelopment() && key == "info")
                return View("Info");

            // Get url for key from cache
            var urlFromCache = await _cache.GetStringAsync(key);
            
            // If the url is found return the url
            if (!(urlFromCache is null))
                return Redirect(urlFromCache);

            try
            {
                // Url not found in cache, try to get it from database.
                var url = await _repository.GetUrlAsync(key);

                // Store the url in the cache
                await _cache.SetStringAsync(key, url, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(0, 1, 0)
                });

                return Redirect(url);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
