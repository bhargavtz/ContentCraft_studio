using ContentCraft_studio.Models;

namespace ContentCraft_studio.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
        Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request);
    }
}