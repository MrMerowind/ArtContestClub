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

        public IActionResult ConfirmEmail()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult SendEmailVeryfication(string to, string verificationCode)
        {
            if(to == null || to == "" || verificationCode == null || verificationCode == "")
            {
                return BadRequest();
            }
            string from = Configuration.GetSection("MailSettings")["Mail"];


            MailMessage message = new MailMessage(from, to);

            string mailbody = "<a href=" + verificationCode + ">Click here to confirm your account on ArtContest.club</a> If you did not register please ignore this email";
            message.Subject = "Confirmation email from ArtContest.club";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            NetworkCredential basicCredential1 = new NetworkCredential(Configuration.GetSection("MailSettings")["Mail"], Configuration.GetSection("MailSettings")["Password"]);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("ConfirmEmail", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}