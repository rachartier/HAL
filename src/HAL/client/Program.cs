using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage;
using HAL.Storage.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace HAL
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStoragePlugin storage = new FileStorage();

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
                    storage.Save(e.Plugin, e.Result);
                });
            }

            pluginManager.ScheldulePlugins(pluginMaster.Plugins);

            while (true) { }
        }
    }
}
