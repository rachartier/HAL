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
                        new Plugin("test/script.py"),
                        new Plugin("test/script.py"),
                        new Plugin("test/script.py"),
                        new Plugin("test/script.pl"),
                        new Plugin("test/script.rb"),
                        new Plugin("test/script.sh"),
                        new Plugin("test/script.go")
                    };

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"Plugin {plugin.FileName} [{plugin.Type}] va etre execute...");
                pluginManager.Run(plugin, storage);
            }

            Console.WriteLine($"\nTaille de la file d'attente: {pluginManager.Executor.QueueLength}\n");
            pluginManager.Executor.WaitForEmptyPool();
            Console.WriteLine($"\nTaille de la file d'attente: {pluginManager.Executor.QueueLength}\n");
        }
    }
}
