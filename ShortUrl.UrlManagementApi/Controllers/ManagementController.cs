using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.AspNetCore.Http;
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

        [HttpGet(Name ="GetAll")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ShortUrlModel>))]
        public async Task<IActionResult> GetAll()
        {
            var shortUrlModels = await _repository.GetAll();

            return new JsonResult(shortUrlModels);
        }

        [HttpGet("{id}", Name ="GetById")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUrlModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long? id)
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

        [HttpPost(Name ="Add")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ShortUrlModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUrl(ShortUrlModel shortUrlModel)
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

        [HttpDelete("{id?}", Name ="DeleteById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(long? id)
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