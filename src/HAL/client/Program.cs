using System.Collections.Generic;
using System;
using System.IO;
using HAL.Storage.Configuration;
using HAL.Storage;
using HAL.Plugin.Mananger;
using HAL.Plugin;
using Newtonsoft.Json.Linq;

namespace HAL
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

            foreach (var plugin in plugins)
            {
                string infos = String.Format("{0,-20}{1,10}{2}{3}", 
                    plugin.FileName, 
                    " ",
                    (plugin.CanBeRun() ? "WORKING" : "NOT WORKING"),
                    (plugin.CanBeRunOnOS() ? "" : " ON THIS OS"));

                Console.WriteLine(infos);

                plugin.OnExecutionFinished += new PluginFile.PluginResultHandler((object o, PluginResultArgs e) =>
                {
                    storage.Save(e.Result);
                });

            }
            pluginManager.ScheldulePlugins(plugins);

            while (true) { }
        }
    }
}
