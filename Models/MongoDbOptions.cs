namespace ContentCraft_studio.Models
{
    public class MongoDbOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ImageDescriptionsCollectionName { get; set; } = string.Empty;
        public string UsersCollectionName { get; set; } = string.Empty;
        public string BusinessNamesCollectionName { get; set; } = string.Empty;
        public string BlogPostsCollectionName { get; set; } = string.Empty;
    }
}
