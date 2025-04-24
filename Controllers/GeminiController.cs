using Microsoft.AspNetCore.Mvc;
using ContentCraft_studio.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using ContentCraft_studio.Services;
using ContentCraft_Studio.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ContentCraft_studio.Controllers
{
    [ApiController]
    [Route("api/gemini")] // Updated base route
    public class GeminiController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger<GeminiController> _logger;

        public GeminiController(IHttpClientFactory clientFactory, IConfiguration configuration, IMongoDbService mongoDbService, ILogger<GeminiController> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Prompt))
            {
                return BadRequest(new { error = "Prompt is required" });
            }
            
            var apiKey = _configuration["Gemini:ApiKey"];
            var modelId = "gemini-2.0-flash-exp-image-generation";

            try
            {
                var client = _clientFactory.CreateClient();
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:streamGenerateContent?key={apiKey}";
                
                var response = await client.PostAsJsonAsync(url, request);
                
                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { error = "Failed to generate image" });

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                if (responseData?.Candidates == null || !responseData.Candidates.Any())
                    return BadRequest(new { error = "No image generated" });

                 var imageUrl = responseData.Candidates.FirstOrDefault()?.Contents?.Parts
                    .FirstOrDefault(p => p.InlineData?.MimeType?.StartsWith("image/") == true)?.InlineData?.Data;

                if (string.IsNullOrEmpty(imageUrl))
                    return BadRequest(new { error = "No image data in response" });

                return Ok(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("api/gemini/save-image-description")]
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
                _logger.LogError(ex, "Error saving image description");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
