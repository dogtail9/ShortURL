using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShortUrl.ManagementGui.Models;

namespace ShortUrl.ManagementGui.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IManagementApiClient _httpCient;

        public HomeController(ILogger<HomeController> logger, IManagementApiClient httpCient)
        {
            _logger = logger;
            _httpCient = httpCient;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<ShortUrlModel> urlData = await _httpCient.GetAllAsync();

            return View(urlData);
        }
        
        // GET: ShortUrl/Create
        [Route("/Home/Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Home/Create")]
        public async Task<IActionResult> Create([Bind("Id,Key,Url")] ShortUrlModel shortUrlModel)
        {
            if (ModelState.IsValid)
            {
                await _httpCient.AddAsync(shortUrlModel);
                return RedirectToAction(nameof(Index));
            }

            return View(shortUrlModel);
        }

        [Route("/Home/Delete/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var shortUrlModel = await _httpCient.GetByIdAsync(id);
            //await _httpCient.DeleteByIdAsync(id);

            return View(shortUrlModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("/Home/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            await _httpCient.DeleteByIdAsync(id);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
