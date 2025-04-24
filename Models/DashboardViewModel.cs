using System;
using System.Collections.Generic;

namespace ContentCraft_studio.Models
{
    public class DashboardViewModel
    {
        public UserModel User { get; set; }
        public int TotalUsage { get; set; }
        public List<UserActivity> RecentActivities { get; set; }

        public DashboardViewModel()
        {
            User = new UserModel();
            RecentActivities = new List<UserActivity>();
        }
    }

    public class UserActivity
    {
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        public UserActivity()
        {
            ActivityType = string.Empty;
            Description = string.Empty;
            Timestamp = DateTime.UtcNow;
        }
    }
}