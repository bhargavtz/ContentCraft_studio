using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ContentCraft_studio.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class GeminiController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public GeminiController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpPost("generate-image-description")]
        public async Task<IActionResult> GenerateImageDescription()
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            try
            {
                var formCollection = await Request.ReadFormAsync();
                var image = formCollection.Files[0];

                if (image == null || image.Length == 0)
                    return BadRequest(new { error = "No image uploaded" });

                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                // Encode the image bytes as base64
                var base64Image = Convert.ToBase64String(imageBytes);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new {
                                    text = "Provide a detailed and accurate description of the following image, including all visible elements, objects, and the overall context. Be as descriptive as possible."
                                },
                                new {
                                    inlineData = new {
                                        mimeType = image.ContentType,
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var client = _clientFactory.CreateClient();
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + apiKey);
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody));
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = $"Gemini API error: {response.StatusCode} - {errorContent}" });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);

                var root = jsonDoc.RootElement;
                string description = "";

                if(root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var contentElem = candidates[0].GetProperty("content");
                    if(contentElem.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        description = parts[0].GetProperty("text").GetString();
                    }
                }

                return Ok(new { description });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}