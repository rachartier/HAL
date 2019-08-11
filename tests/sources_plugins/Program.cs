using System.Collections.Generic;
using System;

namespace TestSourcesPlugins
{
    class Program
    {
        static void Main(string[] args)
        {
            var pluginManager = new PluginManager();
            IStorage storage = new TextStorage();

            var plugins = new List<Plugin>()
            {
                new Plugin("test/pozsjgtezpojt.ronitzorint"),
                    new Plugin("test/dll.dll"),
                    new Plugin("test/script.rb"),
                    new Plugin("test/script.pl"),
                    new Plugin("test/script.sh"),
                    new Plugin("test/script.go"),
                    new Plugin("test/script.py"),
                    new Plugin("test/script.py"),
                    new Plugin("test/script.py"),
            };

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"Plugin {plugin.FileName} [{plugin.Type}] va etre execute...");
                pluginManager.Run(plugin, storage);
            }

            pluginManager.Executor.WaitForEmptyPool();
            Console.WriteLine("Tous les plugins ont ete executes\n\n");
            Console.WriteLine("Tests Schelduler: ");

            ScheldulerService.Instance.SchelduleTask("test_schelduler_1", 0.0006, () =>
            {
                Console.Write("[schelduler 1]");
                pluginManager.Run(plugins[1], storage);
            });
            ScheldulerService.Instance.SchelduleTask("test_schelduler_2", 0.0013, () => { 
					Console.Write("[schelduler 2]");
					pluginManager.Run(plugins[2], storage); });
            ScheldulerService.Instance.SchelduleTask("test_schelduler_3", 0.0026, () =>
            {
                Console.WriteLine("[schelduler 3]");
                pluginManager.Run(plugins[3], storage);
                ScheldulerService.Instance.UnschelduleTask("test_schelduler_3");
            });

            while (true) { }
        }
    }
}
