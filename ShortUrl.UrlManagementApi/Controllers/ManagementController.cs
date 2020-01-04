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

// docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04