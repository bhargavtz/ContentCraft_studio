using System.ComponentModel.DataAnnotations;

namespace ContentCraft_studio.Models
{
    public class UpdateBlogPostRequest
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Blog post title is required")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Blog post content is required")]
        public required string Content { get; set; }
    }
}