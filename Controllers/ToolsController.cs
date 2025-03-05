using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ContentCraft_studio.Models;

namespace ContentCraft_studio.Controllers
{
    public class ToolsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string GEMINI_API_KEY = "AIzaSyAtnxHDSCzBtb6msLptO2yZFumQ8BWEous";
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateText";

        public ToolsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
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
        public async Task<IActionResult> GenerateCaption(string prompt)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"Generate an engaging Instagram caption for: {prompt}" }
                            }
                        }
                    }
                };

                var response = await client.PostAsync(
                    $"{GEMINI_API_URL}?key={GEMINI_API_KEY}",
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
}
