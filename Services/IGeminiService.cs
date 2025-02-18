namespace GeminiAspNetDemo.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
    }
} 