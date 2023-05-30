using Microsoft.Extensions.Configuration;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;



namespace Dashboard.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;  
        //private readonly IConfiguration _configuration;
        private readonly IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
             _logger = logger;
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
        /// <summary>
        /// Admin Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
                    //HttpContext.Session.SetString("IsLoggedIn", "true");
                    //HttpContext.Session.SetString("LoggedInEmail", model.Email);

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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Getting data in preview
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Preview()
        {
            try
            {
                // JSON file path
                string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "App_Data", "Article.json");
                List<ArticleModel> articles = new();
                if (System.IO.File.Exists(jsonFilePath))
                {
                    string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                    articles = JsonConvert.DeserializeObject<List<ArticleModel>>(jsonContent) ?? new List<ArticleModel>();

                }
                return View(articles);

                //return NotFound(); // JSON file not found
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Post ContentModel properties in the Json file using ArticleModel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PostPreview(ContentModel model, IFormFile pdfFile)
        {
            try
            {
                    List<ArticleModel> articles = new List<ArticleModel>();
                    List<ArticleModel> Getarticles = new List<ArticleModel>();
              
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Invalid data");
                }
                else
                {
                    // Generate a unique id
                    model.Id = Guid.NewGuid();

                    // Generate a unique file name using title and timestamp
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileName = $"{model.Title.Replace(" ", "-").Replace("+", "-Plus").Replace("#", "-Sharp").Replace(".", "-Dot").Replace("$", "-Dollar")}.html";

                    // Generate the complete HTML source code
                    string htmlSourceCode = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{model.Title}</title>
</head>
<body>
    {model.Content}
</body>
</html>";
                    // Encode the HTML content as Base64
                    string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Content));

                    // Save the HTML source code to the specified path
                    string sourceCodePath = Path.Combine(_webHostEnvironment.WebRootPath, "SourceCode", fileName);
                    System.IO.File.WriteAllText(sourceCodePath, htmlSourceCode);

                    // JSON file path
                    string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "App_Data", "Article.json");
                    if (System.IO.File.Exists(jsonFilePath))
                    {
                        string existingJson = System.IO.File.ReadAllText(jsonFilePath);
                        articles = JsonConvert.DeserializeObject<List<ArticleModel>>(existingJson) ?? new();
                    }

                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        // Generate a unique file name for the PDF file
                        string pdfFileName = $"{model.Title.Replace(" ", "-").Replace("+", "-Plus").Replace("#", "-Sharp").Replace(".", "-Dot").Replace("$", "-Dollar")}.pdf";

                        // Save the PDF file to the specified path
                        string pdfFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "UploadFile", pdfFileName);
                        using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                        {
                            pdfFile.CopyTo(stream);
                        }
                        model.PdfFileName = pdfFileName;
                        //// Add the PDF file name to the ArticleModel
                        //newArticle.PdfFileName = pdfFileName;
                    }

                    ArticleModel newArticle = new ArticleModel
                    {
                        Id = model.Id,
                        Title = model.Title,
                        //Content = model.Content,
                        Link = fileName,
                        PdfFileName = model.PdfFileName
                    };

                    articles.Add(newArticle);

                    string updatedJson = JsonConvert.SerializeObject(articles);
                    Getarticles = JsonConvert.DeserializeObject<List<ArticleModel>>(updatedJson);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);

                    //Get Data from JSON

                    //ViewBag.HtmlFileName = fileName;
                    //ViewBag.EncodedHtmlContent = encodedContent;
                }
                    return RedirectToAction("Preview", Getarticles);
                    //return View("Preview", Getarticles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Delete Data from JSON file using ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PostDelete(Guid id)
        {
            //List<ArticleModel> articles = new List<ArticleModel>();
            try
            {
                string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "App_Data", "Article.json");

                if (System.IO.File.Exists(jsonFilePath))
                {
                    string existingJson = System.IO.File.ReadAllText(jsonFilePath);
                    var articles = JsonConvert.DeserializeObject<List<ArticleModel>>(existingJson) ?? new();


                    ArticleModel articleToDelete = articles.FirstOrDefault(a => a.Id == id);
                    if (articleToDelete != null)
                    {
                        articles.Remove(articleToDelete);

                        // Save the updated list to the JSON file
                        string updatedJson = JsonConvert.SerializeObject(articles);
                        System.IO.File.WriteAllText(jsonFilePath, updatedJson);

                        return RedirectToAction("Preview");
                    }
                }
                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Getting data from delete page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            List<ArticleModel> articles = new List<ArticleModel>();
            try
            {
                string jsonFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "App_Data", "Article.json");

                if (System.IO.File.Exists(jsonFilePath))
                {
                    string existingJson = System.IO.File.ReadAllText(jsonFilePath);
                    articles = JsonConvert.DeserializeObject<List<ArticleModel>>(existingJson) ?? new();
                }

                // Find the article with the given ID
                ArticleModel articleToDelete = articles.FirstOrDefault(a => a.Id == id);
                if (articleToDelete != null)
                {
                    
                    return View(articleToDelete);
                }

                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Getting file name which one we want to update
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult UpdatedEditor(string fileName)
        {
            string htmlFilesPath = Path.Combine(_configuration.GetSection("HTMLPath").Value);
            // Load the HTML file content into the Editor
            string filePath = Path.Combine(htmlFilesPath, fileName);
            string title = Path.GetFileNameWithoutExtension(filePath);
            string content = System.IO.File.ReadAllText(filePath);
            var model = new ContentModel { Title = title, Content = content};
            return View(model);
        }
        /// <summary>
        /// posted updated content on html file
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateHTML(ContentModel model)
        {
            
            if (ModelState.IsValid)
            {
                string htmlFilesPath = Path.Combine(_configuration.GetSection("HTMLPath").Value);

                // Save the updated HTML content to the file
                string filePath = Path.Combine(htmlFilesPath, $"{model.Title}.html");
                System.IO.File.WriteAllText(filePath, model.Content);
                return RedirectToAction("Preview");
            }

            // If the model is not valid, return back to the Editor with the model data
            return View("Editor", model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
} 