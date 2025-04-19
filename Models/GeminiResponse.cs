using System.Text.Json.Serialization;

namespace ContentCraft_studio.Models
{
    public class GeminiResponse
    {
        public required List<GeminiCandidate> Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public required GeminiContent Contents { get; set; }
    }

    public class GeminiContent
    {
        public required List<GeminiPart> Parts { get; set; }
    }

    public class GeminiPart
    {
        [JsonPropertyName("inlineData")]
        public GeminiInlineData? InlineData { get; set; }
    }

    public class GeminiInlineData
    {
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        public string? Data { get; set; }
    }
}
