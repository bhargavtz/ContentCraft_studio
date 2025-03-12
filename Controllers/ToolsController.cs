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

        [HttpPost]
        public async Task<IActionResult> GenerateCaption([FromBody] CaptionRequest request)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var apiKey = _configuration["Gemini:ApiKey"];
                var promptText = $"Generate an engaging Instagram caption for the following post with a {request.Mood} tone: {request.Prompt}";

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
                    $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateText?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(content);

                return Json(new { caption = result?.Candidates?.FirstOrDefault()?.Text ?? "Unable to generate caption" });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate caption", details = ex.Message });
            }
        }
    }

    public class CaptionRequest
    {
        public string Prompt { get; set; }
        public string Mood { get; set; }
    }
}
