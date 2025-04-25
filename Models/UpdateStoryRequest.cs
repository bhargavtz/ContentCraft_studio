using System.ComponentModel.DataAnnotations;

namespace ContentCraft_studio.Models
{
    public class UpdateStoryRequest
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Story content is required")]
        public required string Content { get; set; }
    }
}