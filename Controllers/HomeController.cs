using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;

namespace MyApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public string Index()
        {
            _logger.LogInformation("Index page called");
            _logger.LogWarning("This is warning");
            _logger.LogError("This is error");
            return "HIII";
        }
        public string IndexERROR( int b)
        {
            _logger.LogInformation("Index page called");
            _logger.LogWarning("This is warning");
            _logger.LogError("This is error");
            int a = 0;
            int c = 100 / a;
            return "HIIIError";
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
