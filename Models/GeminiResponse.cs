namespace ContentCraft_studio.Models
{
    public class GeminiResponse
    {
        public required List<GeminiCandidate> Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public required string Text { get; set; }
    }
}
