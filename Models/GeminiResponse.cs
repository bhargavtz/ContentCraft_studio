namespace ContentCraft_studio.Models
{
    public class GeminiResponse
    {
        public List<GeminiCandidate> Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public string Text { get; set; }
    }
}
