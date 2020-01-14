using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShortUrl.ManagementGui.Models;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ShortUrl.ManagementGui.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IManagementApiClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IManagementApiClient httpCient)
        {
            _logger = logger;
            _httpClient = httpCient;
        }

        public async Task<IActionResult> Index()
        {
            ModelsAndClaims modelsAndClaims = await _httpClient.GetAllAsync();

            return View(modelsAndClaims);
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
                await _httpClient.AddAsync(shortUrlModel);
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
            var shortUrlModel = await _httpClient.GetByIdAsync(id);

            return View(shortUrlModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("/Home/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            await _httpClient.DeleteByIdAsync(id);

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

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}
