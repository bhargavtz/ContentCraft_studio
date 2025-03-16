using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ContentCraft_studio.Models;

namespace ContentCraft_studio.Controllers
{
    public class ToolsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public ToolsController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult InstagramCaption()
        {
            return View();
        }

        public IActionResult BusinessName()
        {
            return View();
        }

        [HttpPost]
        [Route("api/tools/generate-business-names")]
        public async Task<IActionResult> GenerateBusinessNames([FromBody] BusinessNameRequest request)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var apiKey = _configuration["Gemini:ApiKey"];

                var promptText = $"Generate 5 unique and creative business names for a {request.Industry} company with these keywords: {request.Keywords}. The names should have a {request.Style} style. Each name should be separated by '---'. Make them memorable and suitable for business use.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = promptText }
                            }
                        }
                    }
                };

                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);

                var text = jsonElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                var names = text?.Split("---", StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim())
                    .ToArray() ?? new[] { "Unable to generate business names" };

                var result = names.Select(name => new
                {
                    name = name,
                    brandIdentity = new
                    {
                        uniquePoints = new[]
                        {
                            "Modern and professional appearance",
                            "Reflects industry values",
                            "Memorable and unique"
                        },
                        industryFit = $"Perfect fit for {request.Industry} industry",
                        keywords = request.Keywords.Split(',').Select(k => k.Trim()).ToArray()
                    },
                    nameMeaning = $"This name combines elements that represent {request.Industry} excellence with professional style"
                }).ToArray();

                return Json(new { names = result });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate business names", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCaption([FromForm] CaptionRequest request, IFormFile image)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var apiKey = _configuration["Gemini:ApiKey"];
                string promptText;
                object requestBody;

                if (image != null)
                {
                    using var ms = new MemoryStream();
                    await image.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    var base64Image = Convert.ToBase64String(imageBytes);

                    promptText = $"Analyze this image and generate 5 different catchy Instagram captions with a {request.Mood} tone. Each caption should be unique, include relevant emojis and hashtags, and be separated by '---'. Keep them concise and engaging.";

                    requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new object[]
                                {
                                    new
                                    {
                                        inlineData = new
                                        {
                                            mimeType = image.ContentType,
                                            data = base64Image
                                        }
                                    },
                                    new { text = promptText }
                                }
                            }
                        }
                    };
                }
                else
                {
                    promptText = $"Generate 5 different catchy Instagram captions about {request.Prompt} with a {request.Mood} tone. Each caption should be unique, include relevant emojis and hashtags, and be separated by '---'. Keep them concise and engaging.";

                    requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = promptText }
                                }
                            }
                        }
                    };
                }

                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);

                var text = jsonElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                var captions = text?.Split("---", StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToArray() ?? new[] { "Unable to generate captions" };

                return Json(new { captions });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate caption", details = ex.Message });
            }
        }
    }

    public class CaptionRequest
    {
        public string? Prompt { get; set; }
        public string Mood { get; set; }
    }

    public class BusinessNameRequest
    {
        public string Industry { get; set; }
        public string Keywords { get; set; }
        public string Style { get; set; }
    }
}
