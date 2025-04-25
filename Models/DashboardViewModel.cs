using System;
using System.Collections.Generic;

namespace ContentCraft_studio.Models
{
    public class DashboardViewModel
    {
        public UserModel User { get; set; }
        public int TotalUsage { get; set; }
        public List<UserActivity> RecentActivities { get; set; }
        public List<UserActivity> UserActivities { get; set; }

        public DashboardViewModel()
        {
            User = new UserModel();
            RecentActivities = new List<UserActivity>();
            UserActivities = new List<UserActivity>();
        }
    }
}
