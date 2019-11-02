using HAL.Plugin;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class StorageMongoDB : IStoragePlugin
    {
        private MongoClient client;
        private IMongoDatabase defaultDatabase;

        public void Init(string connectionString)
        {
            client = new MongoClient(connectionString);
            defaultDatabase = client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            var anonymousObject = JObject.Parse(obj.ToString());

            var collection = defaultDatabase.GetCollection<dynamic>(plugin.Infos.Name);
            //await collection.InsertOneAsync(anonymousObject);

            var document = BsonSerializer.Deserialize<dynamic>(obj.ToString());

            await collection.InsertOneAsync(document,
                new InsertOneOptions
                {
                    BypassDocumentValidation = true
                });

            return StorageCode.Success;
        }

        private bool IsJson(string json)
        {
            try
            {
                var test = JObject.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}