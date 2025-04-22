namespace ContentCraft_studio.Models
{
    public class MongoDbOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string ImageDescriptionsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string BusinessNamesCollectionName { get; set; }
        public string BlogPostsCollectionName { get; set; }
        public string StoriesCollectionName { get; set; }
    }
}
