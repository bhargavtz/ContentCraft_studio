using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using Microsoft.AspNetCore.Authorization;
using ContentCraft_Studio.Models;  // Add this for ImageDescription model
using System.Security.Claims;

namespace ContentCraft_studio.Controllers
{
    public class ToolsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IGeminiService _geminiService;
        private readonly IMongoDbService _mongoDbService;

        public ToolsController(IHttpClientFactory clientFactory, IConfiguration configuration, 
            IGeminiService geminiService, IMongoDbService mongoDbService)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _geminiService = geminiService;
            _mongoDbService = mongoDbService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult InstagramCaption()
        {
            return View();
        }

        public IActionResult ImageGenerator()
        {
            return View();
        }

        [HttpPost]
        [Route("api/tools/generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            var result = await _geminiService.GenerateImageAsync(request);
            return Json(result);
        }

        public IActionResult BusinessName()
        {
            return View();
        }

        public IActionResult BlogGenerator()
        {
            return View();
        }

        public IActionResult ImageDescription()
        {
            return View();
        }

        [HttpPost]
        [Route("api/tools/generate-image-description")]
        public async Task<IActionResult> GenerateImageDescription([FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { error = "Please upload an image" });
                }

                if (image.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest(new { error = "Image size must be less than 5MB" });
                }

                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                var client = _clientFactory.CreateClient();
                var apiKey = _configuration["Gemini:ApiKey"];

                var promptText = "Analyze this image and provide a detailed, professional description. Include: " +
                    "1. Main subject and setting\n" +
                    "2. Notable visual elements and composition\n" +
                    "3. Colors, lighting, and atmosphere\n" +
                    "4. Any text or recognizable elements\n" +
                    "5. Overall mood or impression";

                var requestBody = new
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

                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { error = "Failed to generate description" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);

                var description = jsonElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Ok(new { description = description ?? "Unable to generate description" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to generate description", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("api/tools/save-image-description")]
        public async Task<IActionResult> SaveImageDescription([FromBody] SaveImageDescriptionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Description))
                {
                    return BadRequest(new { error = "Description is required" });
                }

                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var imageDescription = new ImageDescription
                {
                    UserId = userId,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await _mongoDbService.SaveImageDescriptionAsync(imageDescription);
                return Ok(new { message = "Description saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to save description", details = ex.Message });
            }
        }

        public IActionResult StoryGenerator()
        {
            return View();
        }

        [HttpPost]
        [Route("api/tools/generate-blog-content")]
        public async Task<IActionResult> GenerateBlogContent([FromBody] BlogContentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Prompt))
                {
                    return Json(new { error = "Prompt cannot be empty" });
                }
                var result = await _geminiService.GenerateContentAsync(request.Prompt);
                return Json(new { content = result });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate blog content", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/tools/generate-story")]
        public async Task<IActionResult> GenerateStory([FromBody] StoryGenerationRequest request)
        {
            try
            {
                 if (string.IsNullOrEmpty(request.Prompt))
                {
                    return Json(new { error = "Prompt cannot be empty" });
                }
                var result = await _geminiService.GenerateContentAsync(request.Prompt);
                return Json(new { story = result });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate story", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/tools/generate-business-names")]
        public async Task<IActionResult> GenerateBusinessNames([FromBody] BusinessNameRequest request)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var apiKey = _configuration["Gemini:ApiKey"];

                var promptText = $@"Generate 5 unique, creative, and memorable business names for a company in the {request.Industry} industry. These names should incorporate or relate to the following keywords: {request.Keywords}. The overall style should be {request.Style}.

For each name, provide the following information in a structured format:
1. The business name
2. Brand Identity: List 3 key characteristics that define the brand's personality and values
3. Industry Fit: Explain how the name aligns with {request.Industry} industry
4. Keywords: Highlight which of the provided keywords ({request.Keywords}) are incorporated
5. Name Meaning: Provide a brief explanation of the name's significance and symbolism

The names should:
* Be suitable for use in branding and marketing
* Avoid generic prefixes like 'Company', 'Inc.', 'LLC'
* Be diverse in their approach (some literal, some metaphorical, some playful)
* Focus on the core aspects of the {request.Industry} industry

Separate each complete name entry with '---'. Format each entry as:
Name: [business name]
Brand Identity: [3 bullet points]
Industry Fit: [explanation]
Keywords: [relevant keywords]
Name Meaning: [brief explanation]";
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

                var entries = text?.Split("---", StringSplitOptions.RemoveEmptyEntries)
                    .Select(entry =>
                    {
                        var lines = entry.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        var nameData = new Dictionary<string, string>();

                        foreach (var line in lines)
                        {
                            var parts = line.Split(':', 2);
                            if (parts.Length == 2)
                            {
                                nameData[parts[0].Trim()] = parts[1].Trim();
                            }
                        }

                        return nameData;
                    })
                    .ToArray() ?? Array.Empty<Dictionary<string, string>>();

                var result = entries.Select(entry => new
                {
                    name = entry.GetValueOrDefault("Name", "Unnamed"),
                    brandIdentity = new
                    {
                        uniquePoints = entry.GetValueOrDefault("Brand Identity", "")
                            .Split('•', StringSplitOptions.RemoveEmptyEntries)
                            .Select(point => point.Trim())
                            .Where(point => !string.IsNullOrEmpty(point))
                            .ToArray(),
                        industryFit = entry.GetValueOrDefault("Industry Fit", $"Perfect fit for {request.Industry} industry"),
                        keywords = entry.GetValueOrDefault("Keywords", request.Keywords)
                            .Split(',').Select(k => k.Trim()).ToArray()
                    },
                    nameMeaning = entry.GetValueOrDefault("Name Meaning", 
                        $"This name combines elements that represent {request.Industry} excellence with professional style")
                }).ToArray();

                if (!result.Any())
                {
                    result = new[] { new 
                    {
                        name = "Unable to generate business names",
                        brandIdentity = new
                        {
                            uniquePoints = new string[] { },
                            industryFit = string.Empty,
                            keywords = new string[] { }
                        },
                        nameMeaning = string.Empty
                    }};
                }

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
        public required string Mood { get; set; }
    }

    public class BusinessNameRequest
    {
        public required string Industry { get; set; }
        public required string Keywords { get; set; }
        public required string Style { get; set; }
    }

    public class BlogContentRequest
    {
        public required string Prompt { get; set; }
    }
}
