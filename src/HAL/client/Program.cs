using System.Collections.Generic;
using System;
using System.IO;
using HAL.Storage.Configuration;
using HAL.Storage;
using HAL.Plugin.Mananger;
using HAL.Plugin;
using Newtonsoft.Json.Linq;
using System.Text;
using NLog;
using NLog.Fluent;

namespace HAL
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStorage storage = new TextStorage();

            var pluginMaster = new PluginMaster();
            var pluginManager = new PluginManager(pluginMaster);

            configFile.SetScriptExtensionsConfiguration(pluginMaster);
            configFile.SetInterpreterNameConfiguration(pluginMaster);

            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginMaster.AddPlugin(file);
            }

            configFile.SetPluginsConfiguration(pluginMaster.Plugins);

            foreach (var plugin in pluginMaster.Plugins)
            {
                plugin.OnExecutionFinished += new PluginFile.PluginResultHandler((o, e) =>
                {
                    storage.Save(e.Result);
                });

            }
            pluginManager.ScheldulePlugins(pluginMaster.Plugins);

            while (true) { }
        }
    }
}
