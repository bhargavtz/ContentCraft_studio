using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using GeminiAspNetDemo.Models;

namespace GeminiAspNetDemo.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(IOptions<GeminiOptions> options, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = options.Value.ApiKey;
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var requestJson = JsonSerializer.Serialize(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"models/gemini-pro:generateContent?key={_apiKey}",
                    content);

                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                return responseObj
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response generated";
            }
            catch (Exception ex)
            {
                return $"Error generating content: {ex.Message}";
            }
        }
    }
} 