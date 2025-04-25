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
        Task<string> SaveCaptionAsync(Caption caption);
        Task SavePaymentAsync(PaymentModel payment);
        Task<int> GetUserTotalUsageAsync(string userId);
        Task<List<UserActivity>> GetUserRecentActivitiesAsync(string userId);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId);
        Task DeleteUserActivityAsync(string id);
    }
}
