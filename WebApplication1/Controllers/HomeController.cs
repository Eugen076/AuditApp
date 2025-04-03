using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services; 

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmailService _emailService; 

        public HomeController(ILogger<HomeController> logger, EmailService emailService) // ✅ Injectăm serviciul în constructor
        {
            _logger = logger;
            _emailService = emailService;
        }

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

        [HttpPost]
        public async Task<IActionResult> SubmitContact(string name, string email, string message)
        {
        

            string subject = "New Contact Form Submission";
            string body = $"<h3>New message from {name} ({email})</h3><p>{message}</p>";


            await _emailService.SendEmailAsync(email, subject, body);


            ViewBag.Message = "Your message has been sent successfully!";
            return View("Contact");
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Maps()
        {
            return View();
        }
        

    }
}
