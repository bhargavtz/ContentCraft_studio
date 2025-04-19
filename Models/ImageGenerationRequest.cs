namespace ContentCraft_studio.Models
{
    public class ImageGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Size { get; set; } = "1024x1024";
    }

    public class ImageGenerationResponse
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}