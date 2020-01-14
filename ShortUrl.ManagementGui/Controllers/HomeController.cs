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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IManagementApiClient _httpCient;

        public HomeController(ILogger<HomeController> logger)//, IManagementApiClient httpCient)
        {
            _logger = logger;
            //_httpClientFactory = httpClientFactory;
            //_httpCient = httpCient;
        }

        public async Task<IActionResult> Index([FromServices] ManagementApiClient httpClient)
        {
            //var accessToken = await HttpContext.GetTokenAsync("access_token");

            //var client = _httpClientFactory.CreateClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //var response = await client.GetStringAsync("http://localhost:5001/Management");

            //var urlData = JsonConvert.DeserializeObject<IEnumerable<ShortUrlModel>>(response);

            IEnumerable<ShortUrlModel> urlData = await httpClient.GetAllAsync();

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
                //var json = JsonConvert.SerializeObject(shortUrlModel);
                //var accessToken = await HttpContext.GetTokenAsync("access_token");

                //var client = _httpClientFactory.CreateClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                //var response = await client.PostAsync("http://localhost:5001/Management", new StringContent(json));

                //var body = await response.Content.ReadAsStringAsync();
                //var urlData = JsonConvert.DeserializeObject<ShortUrlModel>(body);

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

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}
