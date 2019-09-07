using System.Collections.Generic;
using System;
using System.IO;

namespace TestSourcesPlugins
{
    class Program
    {
        static void Main(string[] args)
        {
            JSONConfigFile configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStorage storage = new TextStorage();

            var pluginManager = new PluginManager();
            var plugins = new List<Plugin>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                plugins.Add(new Plugin(file));
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
