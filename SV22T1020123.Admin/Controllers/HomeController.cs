using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SV22T1020123.Admin.Models;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1020123.Admin.Controllers
{
    /// <summary>
    /// các ch?c n?ng c?a trang ch?
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        //Trang ch?/dashboard
        public IActionResult Index()
        {
            return View();
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
