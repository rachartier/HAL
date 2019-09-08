using System.Collections.Generic;
using System;
using System.IO;
using HAL.Storage.Configuration;
using HAL.Storage;
using HAL.Plugin.Mananger;
using HAL.Plugin;
using Newtonsoft.Json.Linq;

namespace TestSourcesPlugins
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStorage storage = new TextStorage();

            var pluginManager = new PluginManager();
            var plugins = new List<PluginFile>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                plugins.Add(new PluginFile(file));
            }

            configFile.SetPluginsConfiguration(plugins);

            Console.WriteLine("---------------------------------------------------------");

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"Plugin {plugin.FileName} [{plugin.Type}] loaded.");
                pluginManager.Run(plugin, storage);
            }
            pluginManager.Executor.WaitForEmptyPool();

            Console.WriteLine("Tous les plugins ont ete executes\n\n");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("Tests Schelduler: ");

            pluginManager.ScheldulePlugins(plugins, storage);

            Console.WriteLine("---------------------------------------------------------");

            while (true) { }
        }
    }
}
