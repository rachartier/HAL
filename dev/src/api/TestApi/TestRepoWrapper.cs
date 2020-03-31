using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using Controllers;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository;
using Xunit;
using Moq;
using Xunit.Abstractions;

namespace TestApi
{
    public class TestRepoWrapper
    {
        [Fact]
        public void Test_CreateDeletePlugins()
        {
            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseMongoDb(new MongoUrl("mongodb://localhost:27017/repository"));

            var repo = new RepositoryWrapper(new RepositoryContext(dbContextOptionsBuilder.Options));
            var plugin = CreateTestPlugin();
            repo.Plugin.CreatePlugin(plugin);
            
            var results = repo.Plugin.GetPluginByName(plugin.Name);

            Assert.NotNull(results);
            Assert.NotEmpty(results);
            
            repo.Plugin.DeletePlugin(plugin);
        }
        
        [Fact]
        public void Test_GetPluginByName()
        {
            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseMongoDb(new MongoUrl("mongodb://localhost:27017/repository"));

            var repo = new RepositoryWrapper(new RepositoryContext(dbContextOptionsBuilder.Options));
            var plugin = CreateTestPlugin();
            repo.Plugin.CreatePlugin(plugin);
            
            var results = repo.Plugin.GetPluginByName(plugin.Name);

            Assert.NotNull(results);
            Assert.NotEmpty(results);

            foreach (var result in results)
            {
                Assert.Equal(result.Name, plugin.Name);
            }
            
            repo.Plugin.DeletePlugin(plugin);
        }

        [Fact]
        public void Test_GetAllPlugins()
        {
            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseMongoDb(new MongoUrl("mongodb://localhost:27017/repository"));

            var repo = new RepositoryWrapper(new RepositoryContext(dbContextOptionsBuilder.Options));
            IList<Plugin> plugins = new List<Plugin>();

            for (int i = 0; i < 10; i++)
            {
                var plugin = CreateTestPlugin();
                plugins.Add(plugin);
                
                repo.Plugin.CreatePlugin(plugin);
            }
            
            var results = repo.Plugin.GetAllPlugins();

            Assert.NotNull(results);
            Assert.NotEmpty(results);

            foreach (var plugin in plugins)
            {
                repo.Plugin.DeletePlugin(plugin);
            }
        }
        private Plugin CreateTestPlugin()
        {
            var name = Guid.NewGuid().ToString();
            return new Plugin {Id = new ObjectId(), Date = DateTime.Now, Name = name, MachineName = "debian"};
        }
    }
}