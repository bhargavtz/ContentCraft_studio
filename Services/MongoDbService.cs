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
    }

    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<ImageDescription> _imageDescriptions;
        private readonly IMongoCollection<UserModel> _users;
        private readonly ILogger<MongoDbService> _logger;

        public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
        {
            var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDb:DatabaseName"]);
            _imageDescriptions = database.GetCollection<ImageDescription>(configuration["MongoDb:ImageDescriptionsCollectionName"]); // Use config to set the collection name
            _users = database.GetCollection<UserModel>(configuration["MongoDb:UsersCollectionName"]); // Use config to set users collection name
            _logger = logger;
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
    }
}
