using ContentCraft_Studio.Models;
using ContentCraft_studio.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
    }

    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<ImageDescription> _imageDescriptions;
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<BusinessNameModel> _businessNamesCollection;
        private readonly IMongoCollection<BlogPost> _blogPostsCollection;
        private readonly IMongoCollection<Story> _storiesCollection;
        private readonly IMongoCollection<Caption> _captionsCollection;
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
    }
}
