using System.ComponentModel.DataAnnotations;

namespace ContentCraft_studio.Models
{
    public class UpdateCaptionRequest
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Caption text is required")]
        [StringLength(2000, ErrorMessage = "Caption text must not exceed 2000 characters")]
        public required string Text { get; set; }
    }
}