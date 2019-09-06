using System.Collections.Generic;
using System;

namespace TestSourcesPlugins
{
    class Program
    {
        static void Main(string[] args)
        {

            JSONConfigFile configFile = new JSONConfigFile();
            configFile.Load("test.json");

            var pluginManager = new PluginManager();
            IStorage storage = new TextStorage();

            var plugins = new List<Plugin>()
            {
               // new Plugin("test/pozsjgtezpojt.ronitzorint"),
                 new Plugin("test/test.dll"),
                    new Plugin("testso.so"),
                   // new Plugin("test/script.pl"),
                   // new Plugin("test/script.sh"),
                   new Plugin("test/script.py"),
                   // new Plugin("test/script.py"),
                   // new Plugin("test/script.py"),
            };

            configFile.SetPluginsConfiguration(plugins);

            Console.WriteLine("---------------------------------------------------------");

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"Plugin {plugin.FileName} [{plugin.Type}] va etre execute...");
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
