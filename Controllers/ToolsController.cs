using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ContentCraft_studio.Models;
using ContentCraft_studio.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ContentCraft_studio.Controllers
{
    public class ToolsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IGeminiService _geminiService;
        private readonly IMongoDbService _mongoDbService;
        public readonly ILogger<ToolsController> _logger;

        public ToolsController(IHttpClientFactory clientFactory, IConfiguration configuration, 
            IGeminiService geminiService, IMongoDbService mongoDbService, ILogger<ToolsController> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _geminiService = geminiService;
            _mongoDbService = mongoDbService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Tools/InstagramCaption")]
        public IActionResult InstagramCaption()
        {
            return View();
        }

        [HttpPost]
        [Route("Tools/SaveCaption")]
        public async Task<IActionResult> SaveCaption([FromBody] Caption caption)
        {
            try
            {
                if (string.IsNullOrEmpty(caption.Text))
                {
                    return Json(new { success = false, error = "Caption text is required" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User must be logged in to save captions" });
                }

                caption.UserId = userId;
                var captionId = await _mongoDbService.SaveCaptionAsync(caption);

                return Json(new { success = true, captionId = captionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving caption");
                return Json(new { success = false, error = "An error occurred while saving the caption" });
            }
        }

        public IActionResult ImageGenerator()
        {
            return View();
        }

        [HttpPost]
        [Route("api/tools/generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Prompt))
                {
                    _logger.LogError("Invalid image generation request: Prompt is required");
                    return BadRequest(new { error = "Prompt is required" });
                }

                _logger.LogInformation("Generating image with prompt: {Prompt}", request.Prompt);
                var result = await _geminiService.GenerateImageAsync(request);

                if (!result.Success)
                {
                    _logger.LogError("Image generation failed: {Error}", result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Image generated successfully");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while generating the image");
                return StatusCode(500, new { error = "An error occurred while generating the image", details = ex.Message });
            }
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
                return Json(new { error = "Failed to generate business names", details = ex.Message });
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

        [Authorize]
        [HttpPost]
        [Route("api/tools/generate-blog-content")]
        public async Task<IActionResult> GenerateBlogContent(string topic, string keywords, string format, string tone, int length, string targetAudience)
        {
            try
            {
                if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(keywords))
                {
                    return Json(new { error = "Topic and keywords cannot be empty" });
                }

                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                string prompt = $"Generate a {length}-word {format} about {topic} with a {tone} tone for a {targetAudience} audience. The keywords are: {keywords}. The content should be well-written, engaging, and informative. It should also be optimized for SEO.";

                _logger.LogInformation("Prompt: {Prompt}", prompt);

                var content = await _geminiService.GenerateContentAsync(prompt);

                _logger.LogInformation("Content: {Content}", content);

                if (content.StartsWith("Error generating content:"))
                {
                    return Json(new { error = content });
                }

                // Create a new model to store the blog data
                var blogPost = new BlogPost
                {
                    UserId = userId,
                    Title = topic, // You might want to generate a title from the prompt or content
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                // Save the blog post to MongoDB
                await _mongoDbService.SaveBlogPostAsync(blogPost);

                // Calculate SEO score
                double seoScore = CalculateSEOScore(content, keywords);

                return Json(new { content = content, message = "Blog post saved successfully", seoScore = seoScore });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate blog content");
                return Json(new { error = "Failed to generate blog content", details = ex.Message });
            }
        }

        private double CalculateSEOScore(string content, string keywords)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keywords))
            {
                return 0;
            }

            string[] keywordList = keywords.Split(',').Select(k => k.Trim()).ToArray();
            double matchedKeywords = 0;

            foreach (string keyword in keywordList)
            {
                if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    matchedKeywords++;
                }
            }

            return (matchedKeywords / keywordList.Length) * 100;
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

                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                _logger.LogInformation("Story Prompt: {Prompt}", request.Prompt);

                var result = await _geminiService.GenerateContentAsync(request.Prompt);

                _logger.LogInformation("Story Result: {Result}", result);

                // Create a new model to store the story data
                var story = new Story
                {
                    UserId = userId,
                    Prompt = request.Prompt,
                    Content = result,
                    CreatedAt = DateTime.UtcNow
                };

                // Save the story to MongoDB
                await _mongoDbService.SaveStoryAsync(story);

                return Json(new { story = result, message = "Story saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate story", ex);
                return Json(new { error = "Failed to generate story", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/tools/generate-business-names")]
        public async Task<IActionResult> GenerateBusinessNames([FromBody] BusinessNameRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "User ID is required" });
                }

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

                _logger.LogInformation("Sending request to Gemini API: {RequestBody}", JsonSerializer.Serialize(requestBody));
                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                _logger.LogInformation("Received response from Gemini API: {StatusCode}", (int)response.StatusCode);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response content from Gemini API: {Content}", content);
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

                // Map the generated business names to the BusinessNameModel and save them to the database
                var businessNamesToSave = result.Select(name => new BusinessNameModel
                {
                    UserId = userId,
                    Name = name.name,
                    BrandIdentity = new BrandIdentity
                    {
                        UniquePoints = name.brandIdentity.uniquePoints,
                        IndustryFit = name.brandIdentity.industryFit,
                        Keywords = name.brandIdentity.keywords
                    },
                    NameMeaning = name.nameMeaning,
                    CreatedAt = DateTime.UtcNow
                });

                foreach (var businessName in businessNamesToSave)
                {
                    try
                    {
                        await _mongoDbService.SaveBusinessNameAsync(businessName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save business name to database: {ErrorMessage}", ex.Message);
                        return Json(new { error = "Failed to generate business names", details = "An error occurred while saving the business names. Please try again." });
                    }
                }

                return Json(new { names = result });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to generate business names", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GenerateCaption([FromForm] CaptionRequest request, IFormFile image)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { error = "User must be logged in to generate captions" });
                }

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

                _logger.LogInformation("Sending request to Gemini API for caption generation");
                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode}, {Error}", response.StatusCode, errorContent);
                    return Json(new { error = $"Failed to generate caption: {response.StatusCode} - {errorContent}" });
                }
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

                // Save captions to database
                foreach (var captionText in captions)
                {
                    var caption = new Caption
                    {
                        UserId = userId,
                        Text = captionText,
                        Mood = request.Mood,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _mongoDbService.SaveCaptionAsync(caption);
                }

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
