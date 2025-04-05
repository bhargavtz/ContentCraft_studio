using GeminiAspNetDemo.Models;

namespace GeminiAspNetDemo.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
        Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request);
    }
}