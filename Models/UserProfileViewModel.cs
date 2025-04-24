using System;

namespace ContentCraft_studio.Models
{
    public class UserProfileViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public DateTime JoinDate { get; set; }
    }
}