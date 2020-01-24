using System.ComponentModel.DataAnnotations.Schema;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities.Models
{
    [MongoCollection("Plugin")]
    public class Plugin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("machine_name")]
        public string MachineName { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("result")]
        public BsonDocument Result { get; set; }
    }
}
