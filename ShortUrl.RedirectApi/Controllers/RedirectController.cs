using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
