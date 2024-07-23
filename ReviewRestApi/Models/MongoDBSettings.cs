using ReviewRestApi.Models.Interface;

namespace ReviewRestApi.Models
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
