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

        private static readonly SemaphoreSlim _throttler = new SemaphoreSlim(5, 5); // Limit to 5 concurrent requests
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private const int MaxRetries = 3;

        public async Task<string> GenerateContentAsync(string prompt)
        {
            try
            {
                await _throttler.WaitAsync(_timeout);
                
                for (int attempt = 0; attempt <= MaxRetries; attempt++)
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
                            },
                            generationConfig = new
                            {
                                temperature = 0.7, // Reduced for more focused output
                                topK = 40,
                                topP = 0.95,
                                maxOutputTokens = 4096
                            },
                            safetySettings = new object[] { }
                        };

                        var requestJson = JsonSerializer.Serialize(request);
                        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                        
                        using var cts = new CancellationTokenSource(_timeout);
                        var response = await _httpClient.PostAsync(
                            $"models/gemini-2.0-flash:generateContent?key={_apiKey}",
                            content,
                            cts.Token);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            if (attempt < MaxRetries)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
                                continue;
                            }
                            throw new Exception("Rate limit exceeded. Please try again later.");
                        }

                        response.EnsureSuccessStatusCode();
                        
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                        
                        var responseText = responseObj
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString() ?? "No response generated";
                            
                        // Preserve markdown formatting in the response
                        responseText = responseText
                            .Replace("##", "\n##")  // Ensure headers have newlines
                            .Replace("**", "\n**")  // Ensure bold text has newlines
                            .Replace("*", "\n*")   // Ensure italic and list items have newlines
                            .Replace("-", "\n-")   // Ensure list items have newlines
                            .Replace(">", "\n>")   // Ensure blockquotes have newlines
                            .Replace("```", "\n```"); // Ensure code blocks have newlines

                        // Remove any extra newlines and normalize spacing
                        responseText = string.Join("\n", 
                            responseText.Split('\n')
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrWhiteSpace(line)));

                        return responseText;
                    }
                    catch (Exception) when (attempt < MaxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
                        continue;
                    }
                }
                throw new Exception("Failed to generate content after multiple attempts.");
            }
            catch (Exception ex)
            {
                return $"Error generating content: {ex.Message}";
            }
            finally
            {
                _throttler.Release();
            }
        }

        public async Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request)
        {
            try
            {
                var geminiRequest = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = request.Prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.4,
                        topK = 32,
                        topP = 1,
                        maxOutputTokens = 2048
                    },
                    safetySettings = new object[] { }
                };

                var requestJson = JsonSerializer.Serialize(geminiRequest);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"models/gemini-pro-vision:generateContent?key={_apiKey}",
                    content);

                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var contentParts = responseObj
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                var generatedContent = new List<string>();

                foreach (var part in contentParts.EnumerateArray())
                {
                    generatedContent.Add(part.GetProperty("text").GetString());
                }

                var imageUrl = string.Join(" ", generatedContent);

                return new ImageGenerationResponse
                {
                    ImageUrl = imageUrl ?? string.Empty,
                    Success = !string.IsNullOrEmpty(imageUrl)
                };
            }
            catch (Exception ex)
            {
                return new ImageGenerationResponse
                {
                    Error = $"Error generating image: {ex.Message}",
                    Success = false
                };
            }
        }
    }
}