using System;

namespace ContentCraft_studio.Models
{
    public class UserModel
    {
        public string Id { get; set; } // Corresponds to NAMEIDENTIFIER
        public string Name { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public DateTime LastLogin { get; set; } // Added field for last login timestamp

        public UserModel()
        {
            Id = string.Empty;
            Name = string.Empty;
            Email = string.Empty;
            Nickname = string.Empty;
        }
    }
}
