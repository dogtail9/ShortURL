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

// docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04