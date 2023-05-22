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
using System.IO;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Channels;

namespace Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration; 
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
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
        public IActionResult Preview()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
        /// <summary>
        /// Delete data in json file using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            try
            {
                string jsonFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/App_Data", "Article.json");
                List<ArticleModel> articles = new List<ArticleModel>();

                if (System.IO.File.Exists(jsonFilePath))
                {
                    string existingJson = System.IO.File.ReadAllText(jsonFilePath);
                    articles = JsonConvert.DeserializeObject<List<ArticleModel>>(existingJson) ?? new();
                }

                // Find the article with the given ID
                ArticleModel articleToDelete = articles.FirstOrDefault(a => a.Id == id);
                if (articleToDelete != null)
                {
                    articles.Remove(articleToDelete);

                    // Save the updated list to the JSON file
                    string updatedJson = JsonConvert.SerializeObject(articles);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);
                }

                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Admin Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
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
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Post ContentModel properties in the Json file using ArticleModel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Preview(ContentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Invaild data");
                }
                else
                {
                    //Generate a unique id
                    model.Id = Guid.NewGuid();

                    // Generate a unique file name or use a specific naming convention
                    string fileName = model.Title.Replace(" ", "-") + ".html";

                    // Encode the HTML content as Base64
                    string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Content));
                    
                    //JSON file path
                    string jsonFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/App_Data", "Article.json");
                    List<ArticleModel> articles = new List<ArticleModel>();

                    if (System.IO.File.Exists(jsonFilePath))
                    {
                        string existingJson = System.IO.File.ReadAllText(jsonFilePath);
                        articles = JsonConvert.DeserializeObject<List<ArticleModel>>(existingJson)??new();
                    }

                    ArticleModel newArticle = new ArticleModel
                    {
                        Id = model.Id,
                        Title = model.Title,
                        Content = model.Content,
                        Link = fileName

                    };

                    articles.Add(newArticle);

                    string updatedJson = JsonConvert.SerializeObject(articles);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);

                    //Get Data from json

                    ViewBag.HtmlFileName = fileName;
                    ViewBag.EncodedHtmlContent = encodedContent;
                    return View("Preview", articles);
                }
                return Ok();
        }
        catch(Exception ex)
        {
             return BadRequest(ex);
        }
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
        
    }
}