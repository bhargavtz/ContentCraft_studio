using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ContentCraft_studio.Models;

namespace ContentCraft_studio.Services
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
                            
                        // Return the response text directly without any modifications
                        // This will preserve the original markdown formatting from Gemini
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
                    prompt = request.Prompt,
                    model = "gemini-2.0-flash-exp-image-generation",
                    parameters = new
                    {
                        temperature = 0.4,
                        topK = 32,
                        topP = 1,
                        imageSize = "1024x1024",
                        sampleCount = 1
                    }
                };

                var requestJson = JsonSerializer.Serialize(geminiRequest);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"models/gemini-2.0-flash-exp-image-generation:generateImage?key={_apiKey}",
                    content);

                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var imageData = responseObj
                    .GetProperty("images")[0]
                    .GetProperty("data")
                    .GetString();

                var imageUrl = imageData;

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
