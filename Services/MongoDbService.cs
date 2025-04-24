using ContentCraft_studio.Models; // Update to correct namespace
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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
    }

    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<ImageDescription> _imageDescriptions;
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<BusinessNameModel> _businessNamesCollection;
        private readonly IMongoCollection<BlogPost> _blogPostsCollection;
        private readonly IMongoCollection<Story> _storiesCollection;
        private readonly IMongoCollection<Caption> _captionsCollection;
        private readonly IMongoCollection<PaymentModel> _paymentsCollection;
        private readonly ILogger<MongoDbService> _logger;

        public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var mongoDbOptions = configuration.GetSection("MongoDB").Get<MongoDbOptions>() 
                ?? throw new InvalidOperationException("MongoDB configuration is missing");
                
            if (string.IsNullOrEmpty(mongoDbOptions.ConnectionString))
                throw new InvalidOperationException("MongoDB connection string is not configured");
            if (string.IsNullOrEmpty(mongoDbOptions.DatabaseName))
                throw new InvalidOperationException("MongoDB database name is not configured");

            var client = new MongoClient(mongoDbOptions.ConnectionString);
            var database = client.GetDatabase(mongoDbOptions.DatabaseName);
            
            _imageDescriptions = database.GetCollection<ImageDescription>(
                mongoDbOptions.ImageDescriptionsCollectionName ?? "ImageDescriptions");
            _users = database.GetCollection<UserModel>(
                mongoDbOptions.UsersCollectionName ?? "Users");
            _businessNamesCollection = database.GetCollection<BusinessNameModel>(
                mongoDbOptions.BusinessNamesCollectionName ?? "BusinessNames");
            _blogPostsCollection = database.GetCollection<BlogPost>(
                mongoDbOptions.BlogPostsCollectionName ?? "BlogPosts");
            _storiesCollection = database.GetCollection<Story>(
                mongoDbOptions.StoriesCollectionName ?? "Stories");
            _captionsCollection = database.GetCollection<Caption>("Captions");
            _paymentsCollection = database.GetCollection<PaymentModel>("Payments");
        }

        public async Task SaveImageDescriptionAsync(ImageDescription imageDescription)
        {
            try
            {
                _logger.LogInformation("Attempting to save image description: {@ImageDescription}", imageDescription);
                await _imageDescriptions.InsertOneAsync(imageDescription);
                _logger.LogInformation("Image description saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image description.");
                throw;
            }
        }

        public async Task UpsertUserAsync(UserModel user)
        {
            try
            {
                _logger.LogInformation("Attempting to upsert user: {@User}", user);
                var filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
                var options = new ReplaceOptions { IsUpsert = true };
                await _users.ReplaceOneAsync(filter, user, options);
                _logger.LogInformation("User upserted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert user: {Ex}", ex);
                throw;
            }
        }

        public async Task SaveBusinessNameAsync(BusinessNameModel businessName)
        {
            try
            {
                _logger.LogInformation("Attempting to save business name: {@BusinessName}", businessName);
                await _businessNamesCollection.InsertOneAsync(businessName);
                _logger.LogInformation("Business name saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save business name: {Ex}", ex);
                throw;
            }
        }

        public async Task SaveBlogPostAsync(BlogPost blogPost)
        {
            try
            {
                _logger.LogInformation("Attempting to save blog post: {@BlogPost}", blogPost);
                await _blogPostsCollection.InsertOneAsync(blogPost);
                _logger.LogInformation("Blog post saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save blog post: {Ex}", ex);
                throw;
            }
        }

        public async Task SaveStoryAsync(Story story)
        {
            try
            {
                _logger.LogInformation("Attempting to save story: {@Story}", story);
                await _storiesCollection.InsertOneAsync(story);
                _logger.LogInformation("Story saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save story: {Ex}", ex);
                throw;
            }
        }

        public async Task<string> SaveCaptionAsync(Caption caption)
        {
            try
            {
                _logger.LogInformation("Attempting to save caption: {@Caption}", caption);
                await _captionsCollection.InsertOneAsync(caption);
                _logger.LogInformation("Caption saved successfully.");
                return caption.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save caption: {Ex}", ex);
                throw;
            }
        }

        public async Task SavePaymentAsync(PaymentModel payment)
        {
            try
            {
                _logger.LogInformation("Attempting to save payment: {@Payment}", payment);
                await _paymentsCollection.InsertOneAsync(payment);
                _logger.LogInformation("Payment saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save payment: {Ex}", ex);
                throw;
            }
        }

        public async Task<int> GetUserTotalUsageAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting total usage for user: {UserId}", userId);
                
                var imageCount = await _imageDescriptions.CountDocumentsAsync(x => x.UserId == userId);
                var businessNameCount = await _businessNamesCollection.CountDocumentsAsync(x => x.UserId == userId);
                var blogPostCount = await _blogPostsCollection.CountDocumentsAsync(x => x.UserId == userId);
                var storyCount = await _storiesCollection.CountDocumentsAsync(x => x.UserId == userId);
                var captionCount = await _captionsCollection.CountDocumentsAsync(x => x.UserId == userId);
                
                var totalUsage = (int)(imageCount + businessNameCount + blogPostCount + storyCount + captionCount);
                
                _logger.LogInformation("Total usage for user {UserId}: {TotalUsage}", userId, totalUsage);
                return totalUsage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total usage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserActivity>> GetUserRecentActivitiesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting recent activities for user: {UserId}", userId);
                var activities = new List<UserActivity>();

                // Get recent image descriptions
                var images = await _imageDescriptions
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();
                activities.AddRange(images.Select(x => new UserActivity
                {
                    Type = "Image Description",
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                }));

                // Get recent business names
                var businessNames = await _businessNamesCollection
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();
                activities.AddRange(businessNames.Select(x => new UserActivity
                {
                    Type = "Business Name",
                    Description = x.Name ?? string.Empty,
                    CreatedAt = x.CreatedAt
                }));

                // Get recent blog posts
                var blogPosts = await _blogPostsCollection
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();
                activities.AddRange(blogPosts.Select(x => new UserActivity
                {
                    Type = "Blog Post",
                    Description = x.Title,
                    CreatedAt = x.CreatedAt
                }));

                // Return activities sorted by creation date
                var sortedActivities = activities
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(10)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} recent activities for user {UserId}", 
                    sortedActivities.Count, userId);
                return sortedActivities ?? new List<UserActivity>(); // Ensure non-null return
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent activities for user {UserId}", userId); // Correct LogError usage
                throw;
            }
        }
    }
}
