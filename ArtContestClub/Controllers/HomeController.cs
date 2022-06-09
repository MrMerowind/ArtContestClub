using ArtContestClub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ArtContestClub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration Configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }

        public IActionResult ConfirmEmail()
        {
            return View();
        }

        public IActionResult ConfirmEmailFailed()
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