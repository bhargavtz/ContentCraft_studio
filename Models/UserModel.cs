using System;

namespace ContentCraft_studio.Models
{
    public class UserModel
    {
        public string Id { get; set; } // Corresponds to NAMEIDENTIFIER
        public string Name { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool EmailVerified { get; set; }
        public long AuthTime { get; set; }
    }
}