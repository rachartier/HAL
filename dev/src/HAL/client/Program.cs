﻿using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage;
using HAL.Storage.Configuration;
using Newtonsoft.Json.Linq;
using Plugin.Manager;
using System.IO;

namespace HAL
{
    public class Program
    {
        private static void Main(string[] args)
        {
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStoragePlugin storage = new TextStorage();
            IPluginMaster pluginMaster = new PluginMasterBasePlugin();

            APluginManager pluginManager = new ScheduledPluginManager(pluginMaster);

            configFile.SetScriptExtensionsConfiguration(pluginMaster);
            configFile.SetInterpreterNameConfiguration(pluginMaster);

            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginMaster.AddPlugin(file);
                pluginMaster.AddPlugin(file);
                pluginMaster.AddPlugin(file);
            }

            configFile.SetPluginsConfiguration(pluginMaster.Plugins);

            foreach (var plugin in pluginMaster.Plugins)
            {
                plugin.OnExecutionFinished += new BasePlugin.PluginResultHandler((o, e) =>
                {
                    storage.Save(e.Plugin, e.Result);
                });
            }

            (pluginManager as ScheduledPluginManager).SchedulePlugins(pluginMaster.Plugins);

            while (true) { }
        }
    }
}
