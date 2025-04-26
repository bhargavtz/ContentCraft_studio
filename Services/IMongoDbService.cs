using ContentCraft_studio.Models;

namespace ContentCraft_studio.Services
{
    public interface IMongoDbService
    {
        Task SaveImageDescriptionAsync(ImageDescription imageDescription);
        Task UpsertUserAsync(UserModel user);
        Task SaveBusinessNameAsync(BusinessNameModel businessName);
        Task SaveBlogPostAsync(BlogPost blogPost);
        Task SaveStoryAsync(Story story);
        Task UpdateStoryAsync(string id, string content);
        Task DeleteStoryAsync(string id);
        Task<string> SaveCaptionAsync(Caption caption);
        Task UpdateCaptionAsync(string id, string text);
        Task DeleteCaptionAsync(string id);
        Task SavePaymentAsync(PaymentModel payment);
        Task<int> GetUserTotalUsageAsync(string userId);
        Task<List<UserActivity>> GetUserRecentActivitiesAsync(string userId);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId);
        Task DeleteUserActivityAsync(string id);
        Task<DashboardViewModel> GetUserDashboardDataAsync(string userId);
        Task UpdateBlogPostAsync(string id, string title, string content);
        Task DeleteBlogPostAsync(string id);
        Task DeleteImageDescriptionAsync(string id);
        Task DeleteBusinessNameAsync(string id);
        Task UpdateUserAsync(UserModel user);
        Task UpdateUserActivitiesAsync(string userId, UserModel user);
        Task UpdateUserStoriesAsync(string userId, UserModel user);
        Task UpdateUserBlogPostsAsync(string userId, UserModel user);
        Task UpdateUserImageDescriptionsAsync(string userId, UserModel user);
        Task UpdateUserBusinessNamesAsync(string userId, UserModel user);
        Task UpdateImageDescriptionAsync(string id, string description);
        Task<List<BlogPost>> GetAllBlogPostsAsync();
        Task<List<Story>> GetAllStoriesAsync();
        Task<List<Caption>> GetAllCaptionsAsync();
        Task<List<ImageDescription>> GetAllImageDescriptionsAsync();
        Task<List<BusinessNameModel>> GetAllBusinessNamesAsync();
        Task<List<UserActivity>> GetRecentActivitiesAsync(string userId);
    }
}
