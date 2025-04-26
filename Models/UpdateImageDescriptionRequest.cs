using System.ComponentModel.DataAnnotations;

namespace ContentCraft_studio.Models
{
    public class UpdateImageDescriptionRequest
    {
        [Required]
        public string ImageId { get; set; } = string.Empty;

        [Required]
        public string NewDescription { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}