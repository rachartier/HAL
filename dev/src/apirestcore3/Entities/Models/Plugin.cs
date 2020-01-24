using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities.Models
{
    public class Plugin
    {

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("machine_name")]
        public string MachineName { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("result")]
        public dynamic Result { get; set; }
    }
}
