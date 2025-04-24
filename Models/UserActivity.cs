using System;

namespace ContentCraft_studio.Models
{
    public class UserActivity
    {
        public required string Type { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
