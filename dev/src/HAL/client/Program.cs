using HAL.Client;
using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage;
using HAL.Storage.Configuration;
using Newtonsoft.Json.Linq;
using Plugin.Manager;
using System;
using System.IO;

namespace HAL
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ClientFile client = new ClientFile();
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            IStoragePlugin storage = new StorageMongoDB();
            IPluginMaster pluginMaster = new PluginMasterBasePlugin();

            APluginManager pluginManager = new ScheduledPluginManager(pluginMaster);

            string connectionString = configFile.GetDataBaseConnectionString();

            storage.Init(connectionString);

            configFile.SetScriptExtensionsConfiguration(pluginMaster);
            configFile.SetInterpreterNameConfiguration(pluginMaster);

            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginMaster.AddPlugin(file);
            }

            configFile.SetPluginsConfiguration(pluginMaster.Plugins);

            foreach (var plugin in pluginMaster.Plugins)
            {
                plugin.OnExecutionFinished += new System.EventHandler<APlugin.PluginResultArgs>((o, e) =>
                {
                    storage.Save(e.Plugin, e.Result);
                });
            }

            pluginManager.SchedulePlugins(pluginMaster.Plugins);

            client.StartClient();

            while (true) { }
        }
    }
}