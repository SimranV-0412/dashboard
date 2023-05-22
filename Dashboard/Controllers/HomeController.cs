using Microsoft.Extensions.Configuration;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Xml.Linq;
using System;
using Markdig;
using Microsoft.AspNetCore.Hosting.Server;
using System.Text;
using System.Web;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;


namespace Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration; //= new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Editor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the entered credentials match the values in appsettings.json
            string Email = _configuration.GetSection("Email").Value;
            string Password = _configuration.GetSection("Password").Value;

            if (model.Email == Email && model.Password == Password)
            {
                // Authentication succeeded
                // Redirect to the home page or any other protected page
                return RedirectToAction("Editor");
            }
            else
            {
                // Authentication failed
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Preview(ContentModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Invaild data");
            }
            else
            {
                //IEnumerable<ContentModel> obj = new List<ContentModel>();
                // Generate a unique file name or use a specific naming convention
                string fileName = model.Title.Replace(" ", "-") + ".html";

                // Encode the HTML content as Base64
                string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Content));

                // Optionally, you can pass the file path to the view for further processing or display
                ViewBag.HtmlFileName = fileName;
                ViewBag.EncodedHtmlContent = encodedContent;
                string jsonPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Article.json");
                string json = JsonConvert.SerializeObject(model);
                System.IO.File.WriteAllText(jsonPath, json);
                //return RedirectToAction("Index", "Home");
            }
            return View("Preview");
        }
        public IActionResult Preview()
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