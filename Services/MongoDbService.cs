using ContentCraft_studio.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace ContentCraft_studio.Services
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<ImageDescription> _imageDescriptions;
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<BusinessNameModel> _businessNamesCollection;
        private readonly IMongoCollection<BlogPost> _blogPostsCollection;
        private readonly IMongoCollection<Story> _storiesCollection;
        private readonly IMongoCollection<Caption> _captionsCollection;
        private readonly IMongoCollection<PaymentModel> _paymentsCollection;
        private readonly IMongoCollection<UserActivity> _activitiesCollection;
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
            _activitiesCollection = database.GetCollection<UserActivity>("activities");
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
                _logger.LogError(ex, "Failed to save image description: {Ex}", ex);
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
                // Retrieve the inserted caption to get the generated Id
                var filter = Builders<Caption>.Filter.Eq(c => c.Text, caption.Text);
                var insertedCaption = await _captionsCollection.Find(filter).FirstOrDefaultAsync();

                if (insertedCaption != null)
                {
                    return insertedCaption.Id;
                }
                else
                {
                    _logger.LogError("Failed to retrieve inserted caption.");
                    throw new Exception("Failed to retrieve inserted caption after saving.");
                }
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
                _logger.LogError(ex, "Failed to get total usage for user {UserId}: {Ex}", userId, ex);
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
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    ActivityType = "Image Description",
                    Description = x.Description,
                    Timestamp = x.CreatedAt
                }));

                // Get recent business names
                var businessNames = await _businessNamesCollection
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();
                activities.AddRange(businessNames.Select(x => new UserActivity
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    ActivityType = "Business Name",
                    Description = x.Name ?? string.Empty,
                    Timestamp = x.CreatedAt
                }));

                // Get recent blog posts
                var blogPosts = await _blogPostsCollection
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.CreatedAt)
                    .Limit(5)
                    .ToListAsync();
                activities.AddRange(blogPosts.Select(x => new UserActivity
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    ActivityType = "Blog Post",
                    Description = x.Title,
                    Timestamp = x.CreatedAt
                }));

                // Return activities sorted by creation date
                var sortedActivities = activities
                    .OrderByDescending(x => x.Timestamp)
                    .Take(10)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} recent activities for user {UserId}",
                    sortedActivities.Count, userId);
                return sortedActivities ?? new List<UserActivity>(); // Ensure non-null return
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get recent activities for user {UserId}: {Ex}", userId, ex); // Correct LogError usage
                throw;
            }
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting all activities for user: {UserId}", userId);
                var filter = Builders<UserActivity>.Filter.Eq(a => a.UserId, userId);
                var activities = await _activitiesCollection.Find(filter).ToListAsync();
                _logger.LogInformation("Retrieved {Count} activities for user {UserId}", activities.Count, userId);
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get all activities for user {UserId}: {Ex}", ex, userId);
                throw;
            }
        }

        public async Task DeleteUserActivityAsync(string id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete user activity with ID: {Id}", id);
                var filter = Builders<UserActivity>.Filter.Eq(a => a.Id, id);
                await _activitiesCollection.DeleteOneAsync(filter);
                _logger.LogInformation("User activity deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user activity: {Ex}", ex);
                throw;
            }
        }

        public async Task<DashboardViewModel> GetUserDashboardDataAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching dashboard data for user: {UserId}", userId);

                var userFilter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
                var user = await _users.Find(userFilter).FirstOrDefaultAsync();

                var imageDescriptionsFilter = Builders<ImageDescription>.Filter.Eq(i => i.UserId, userId);
                var imageDescriptions = await _imageDescriptions.Find(imageDescriptionsFilter).ToListAsync();

                var businessNamesFilter = Builders<BusinessNameModel>.Filter.Eq(b => b.UserId, userId);
                var businessNames = await _businessNamesCollection.Find(businessNamesFilter).ToListAsync();

                var blogPostsFilter = Builders<BlogPost>.Filter.Eq(b => b.UserId, userId);
                var blogPosts = await _blogPostsCollection.Find(blogPostsFilter).ToListAsync();

                var storiesFilter = Builders<Story>.Filter.Eq(s => s.UserId, userId);
                var stories = await _storiesCollection.Find(storiesFilter).ToListAsync();

                var captionsFilter = Builders<Caption>.Filter.Eq(c => c.UserId, userId);
                var captions = await _captionsCollection.Find(captionsFilter).ToListAsync();

                var totalUsage = await GetUserTotalUsageAsync(userId);
                var recentActivities = await GetUserRecentActivitiesAsync(userId);
                var userActivities = await GetUserActivitiesAsync(userId);

                var dashboardData = new DashboardViewModel
                {
                    User = user ?? new UserModel(),
                    TotalUsage = totalUsage,
                    RecentActivities = recentActivities,
                    UserActivities = userActivities,
                    ImageDescriptions = imageDescriptions,
                    BusinessNames = businessNames,
                    BlogPosts = blogPosts,
                    Stories = stories,
                    InstagramCaptions = captions
                };

                _logger.LogInformation("Dashboard data fetched successfully for user: {UserId}", userId);
                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dashboard data for user: {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateCaptionAsync(string id, string text)
        {
            try
            {
                var filter = Builders<Caption>.Filter.Eq(c => c.Id, id);
                var update = Builders<Caption>.Update.Set(c => c.Text, text);
                
                var result = await _captionsCollection.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Caption not found or not modified");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update caption: {Ex}", ex);
                throw;
            }
        }

        public async Task DeleteCaptionAsync(string id)
        {
            try
            {
                var filter = Builders<Caption>.Filter.Eq(c => c.Id, id);
                var result = await _captionsCollection.DeleteOneAsync(filter);
                
                if (result.DeletedCount == 0)
                {
                    throw new Exception("Caption not found or not deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete caption: {Ex}", ex);
                throw;
            }
        }

        public async Task UpdateStoryAsync(string id, string content)
        {
            try
            {
                var filter = Builders<Story>.Filter.Eq(s => s.Id, id);
                var update = Builders<Story>.Update.Set(s => s.Content, content);
                
                var result = await _storiesCollection.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Story not found or not modified");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update story: {Ex}", ex);
                throw;
            }
        }

        public async Task DeleteStoryAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Story ID cannot be null or empty", nameof(id));
                }

                var story = await _storiesCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
                if (story == null)
                {
                    throw new KeyNotFoundException($"Story with ID {id} was not found");
                }

                var filter = Builders<Story>.Filter.Eq(s => s.Id, id);
                var result = await _storiesCollection.DeleteOneAsync(filter);
                
                if (result.DeletedCount == 0)
                {
                    throw new Exception($"Failed to delete story with ID {id}");
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException)
            {
                _logger.LogWarning(ex, "Validation error in DeleteStoryAsync: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error in DeleteStoryAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task UpdateBlogPostAsync(string id, string title, string content)
        {
            try
            {
                var filter = Builders<BlogPost>.Filter.Eq(bp => bp.Id, id);
                var update = Builders<BlogPost>.Update.Set(bp => bp.Title, title).Set(bp => bp.Content, content);

                var result = await _blogPostsCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Blog post not found or not modified");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update blog post: {Ex}", ex);
                throw;
            }
        }

        public async Task DeleteBlogPostAsync(string id)
        {
            _logger.LogInformation($"Attempting to delete blog post with ID: {id}");
            try
            {
                ObjectId objectId;
                if (ObjectId.TryParse(id, out objectId))
                {
                    var filter = Builders<BlogPost>.Filter.Eq(bp => bp.Id, objectId.ToString());
                    var result = await _blogPostsCollection.DeleteOneAsync(filter);

                    _logger.LogInformation($"Delete result: {result.DeletedCount}");

                    if (result.DeletedCount == 0)
                    {
                        _logger.LogInformation("Blog post not found or not deleted");
                        throw new Exception("Blog post not found or not deleted");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blog post: {Ex}", ex);
                throw;
            }
        }

        public async Task DeleteImageDescriptionAsync(string id)
        {
            await _imageDescriptions.DeleteOneAsync(x => x.Id == id);
        }

        public async Task DeleteBusinessNameAsync(string id)
        {
            await _businessNamesCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<UserModel>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Nickname, user.Nickname);

            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateUserActivitiesAsync(string userId, UserModel user)
        {
            var filter = Builders<UserActivity>.Filter.Eq(a => a.UserId, userId);
            var update = Builders<UserActivity>.Update.Set(a => a.UserName, user.Name);

            await _activitiesCollection.UpdateManyAsync(filter, update);
        }

        public async Task UpdateUserStoriesAsync(string userId, UserModel user)
        {
            var filter = Builders<Story>.Filter.Eq(s => s.UserId, userId);
            var update = Builders<Story>.Update.Set(s => s.UserName, user.Name);

            await _storiesCollection.UpdateManyAsync(filter, update);
        }

        public async Task UpdateUserBlogPostsAsync(string userId, UserModel user)
        {
            var filter = Builders<BlogPost>.Filter.Eq(bp => bp.UserId, userId);
            var update = Builders<BlogPost>.Update.Set(bp => bp.UserName, user.Name);

            await _blogPostsCollection.UpdateManyAsync(filter, update);
        }

        public async Task UpdateUserImageDescriptionsAsync(string userId, UserModel user)
        {
            var filter = Builders<ImageDescription>.Filter.Eq(id => id.UserId, userId);
            var update = Builders<ImageDescription>.Update.Set(id => id.UserName, user.Name);

            await _imageDescriptions.UpdateManyAsync(filter, update);
        }

        public async Task UpdateUserBusinessNamesAsync(string userId, UserModel user)
        {
            var filter = Builders<BusinessNameModel>.Filter.Eq(bn => bn.UserId, userId);
            var update = Builders<BusinessNameModel>.Update.Set(bn => bn.UserName, user.Name);

            await _businessNamesCollection.UpdateManyAsync(filter, update);
        }

        public async Task UpdateImageDescriptionAsync(string id, string description)
        {
            try
            {
                var filter = Builders<ImageDescription>.Filter.Eq(img => img.Id, id);
                var update = Builders<ImageDescription>.Update.Set(img => img.Description, description);

                var result = await _imageDescriptions.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Image description not found or not modified");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update image description: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<BlogPost>> GetAllBlogPostsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all blog posts from the collection.");
                var blogPosts = await _blogPostsCollection.Find(_ => true).ToListAsync();
                return blogPosts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch blog posts: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<Story>> GetAllStoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all stories from the collection.");
                var stories = await _storiesCollection.Find(_ => true).ToListAsync();
                return stories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch stories: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<Caption>> GetAllCaptionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all captions from the collection.");
                var captions = await _captionsCollection.Find(_ => true).ToListAsync();
                return captions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch captions: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<ImageDescription>> GetAllImageDescriptionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all image descriptions from the collection.");
                var descriptions = await _imageDescriptions.Find(_ => true).ToListAsync();
                return descriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch image descriptions: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<BusinessNameModel>> GetAllBusinessNamesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all business names from the collection.");
                var businessNames = await _businessNamesCollection.Find(_ => true).ToListAsync();
                return businessNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch business names: {Ex}", ex);
                throw;
            }
        }

        public async Task<List<UserActivity>> GetRecentActivitiesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching recent activities for user: {UserId}", userId);
                var filter = Builders<UserActivity>.Filter.Eq(a => a.UserId, userId);
                var activities = await _activitiesCollection
                    .Find(filter)
                    .SortByDescending(a => a.Timestamp)
                    .Limit(10)
                    .ToListAsync();
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch recent activities: {Ex}", ex);
                throw;
            }
        }
    }
}
