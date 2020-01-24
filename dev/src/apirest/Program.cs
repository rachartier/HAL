using Entities.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Bson.Serialization;

namespace apirest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BsonClassMap.RegisterClassMap<Plugin>(cm =>
            {
                cm.AutoMap();
            });
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
