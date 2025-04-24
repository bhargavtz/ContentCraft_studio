namespace ContentCraft_studio.Models
{
    public class MongoDbOptions
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string ImageDescriptionsCollectionName { get; set; }
        public required string UsersCollectionName { get; set; }
        public required string BusinessNamesCollectionName { get; set; }
        public required string BlogPostsCollectionName { get; set; }
        public required string StoriesCollectionName { get; set; }
    }
}
