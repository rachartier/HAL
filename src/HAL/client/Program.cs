using System.Collections.Generic;
using System;
using System.IO;
using HAL.Storage.Configuration;
using HAL.Storage;
using HAL.Plugin.Mananger;
using HAL.Plugin;
using Newtonsoft.Json.Linq;
using System.Text;

namespace HAL
{
    class Program
    {
        static void PrintPluginInfos(PluginFile plugin, int indentSize)
        {
            string dotsLine = new string('.', indentSize - plugin.FileName.Length);
            string activated = (plugin.CanBeRun() ? "ACTIVATED" : "NOT ACTIVATED");
            string onThisOs = (plugin.CanBeRunOnOS() ? "" : " ON THIS OS");

            string infos = String.Format($"{plugin.FileName}{dotsLine}{activated}{onThisOs}");

            Console.WriteLine(infos);
        }
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

            foreach (var plugin in plugins)
            {
                PrintPluginInfos(plugin, 30);

                plugin.OnExecutionFinished += new PluginFile.PluginResultHandler((o, e) =>
                {
                    storage.Save(e.Result);
                });

            }
            pluginManager.ScheldulePlugins(plugins);

            while (true) { }
        }
    }
}
