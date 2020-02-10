using System;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using HAL.Plugin;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace HAL.Storage
{
    public class StorageMongoDB : DifferencialStorage, IDatabaseStorage
    {
        private MongoClient client;
        private IMongoDatabase defaultDatabase;

        public override void Init(string connectionString)
        {
            client = new MongoClient(connectionString);
            defaultDatabase = client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        }

        public override async Task<StorageCode> SaveDifferencial<T>(APlugin plugin, T obj)
        {
            var anonymousObject = JObject.Parse(obj.ToString());

            var collection = defaultDatabase.GetCollection<dynamic>("results");
            //await collection.InsertOneAsync(anonymousObject);

            dynamic document = new ExpandoObject();

            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(APlugin).GetProperties(BindingFlags.Public |
                                                          BindingFlags.Static);

            document.name = plugin.Infos.Name;
            document.machine_name = Environment.MachineName;
            document.result = BsonSerializer.Deserialize<dynamic>(obj.ToString());

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