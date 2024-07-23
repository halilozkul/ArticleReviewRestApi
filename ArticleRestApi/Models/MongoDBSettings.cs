using ArticleRestApi.Models.Interface;

namespace ArticleRestApi.Models
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
