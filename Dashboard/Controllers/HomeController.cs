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

        //    var user = config.GetSection("keys").Value;
        //    return View(Editor);

        //public ActionResult Preview(ContentModel model)
        //{
        //    string htmlContent = ProcessToHtml(model.Content);
        //    ViewBag.HtmlContent = htmlContent;
        //    return View("Preview");
        //}


        [HttpPost]
        public IActionResult Preview(ContentModel model)
        {
            string htmlContent = ProcessToHtml(model.Content);
            // Generate a unique file name or use a specific naming convention
            string fileName = model.Title + ".html";

            // Specify the path where the HTML file will be saved
            string filePath = Path.Combine("D:/Project_Dashboard/Dashboard/bin/Debug/net6.0", fileName);
            //Server.MapPath("~/GeneratedFiles/generated.html");

            // Save the HTML content to the file
            System.IO.File.WriteAllText(filePath, htmlContent);

            // Optionally, you can pass the file path to the view for further processing or display
            ViewBag.HtmlFilePath = filePath;

            return View("Preview");
        }
        private string ProcessToHtml(string Content)
        {
            if (Content == null)
            {
                // Handle the case where content is null
                return string.Empty;
            }

            // Perform the conversion logic
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Content);

            // Manipulate or sanitize the HTML content as needed

            return doc.DocumentNode.OuterHtml;
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