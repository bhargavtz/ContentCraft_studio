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
        public List<ImageDescription> ImageDescriptions { get; set; }
        public List<BusinessNameModel> BusinessNames { get; set; }
        public List<BlogPost> BlogPosts { get; set; }
        public List<Story> Stories { get; set; }
        public List<Caption> InstagramCaptions { get; set; }

        public DashboardViewModel()
        {
            User = new UserModel();
            RecentActivities = new List<UserActivity>();
            UserActivities = new List<UserActivity>();
            ImageDescriptions = new List<ImageDescription>();
            BusinessNames = new List<BusinessNameModel>();
            BlogPosts = new List<BlogPost>();
            Stories = new List<Story>();
            InstagramCaptions = new List<Caption>();
        }
    }
}
