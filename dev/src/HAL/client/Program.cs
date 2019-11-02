﻿using HAL.Client;
using HAL.Loggin;
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

            /*
             * Here we instanciate the configuration file
             * Its a JSON format file.
             *
             * All HAL's configration is here.
             */
            IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
            configFile.Load("config/config.json");

            /*
             * A storage is needed to save the output of the plugins
             * here it's a text storage, it means that the output of all plugins
             * will be redirected to the ouput of the console.
             *
             * the only purpose of text storage is to debug and do a showcase.
             *
             * you can switch on local file storage (wich will stock all the outputs on the client side)
             * or by mongodb stockage.
             */
            IStoragePlugin storage = new TextStorage();
            IPluginMaster pluginMaster = new PluginMasterBasePlugin();

            /*
             * The plugin manager will manage and schedule plugins
             *
             * when a plugin need to be executed, PluginManager will launch what the plugin needs and output the result
             * where desired above.
             */
            APluginManager pluginManager = new ScheduledPluginManager(pluginMaster);

            /*
             * A connection string is needed if you want to access a database
             *
             * if none is specified, then it will do nothing and return null.
             */
            string connectionString = configFile.GetDataBaseConnectionString();

            /*
             * If the storage need to be initialized (database...)
             * then it's here.
             */
            storage.Init(connectionString);

            /*
             * Here pluginMaster will receive some customs user specifications,
             * like new extensions or intepreter.
             */
            configFile.SetScriptExtensionsConfiguration(pluginMaster);
            configFile.SetInterpreterNameConfiguration(pluginMaster);

            /*
             * All the plugins in the directory "plugins" will be loaded and added to the plugin master
             */
            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginMaster.AddPlugin(file);
            }

            /*
             * Then the configuration of all of the plugins is set.
             */
            configFile.SetPluginsConfiguration(pluginMaster.Plugins);

            /*
             * An event is added when the plugin's execution is finished to save it where the user specified above.
             */
            foreach (var plugin in pluginMaster.Plugins)
            {
                plugin.OnExecutionFinished += new System.EventHandler<APlugin.PluginResultArgs>((o, e) =>
                {
                    storage.Save(e.Plugin, e.Result);
                });
            }

            /*
             * All the plugins are then schelduled to be launched when needed.
             */
            pluginManager.SchedulePlugins(pluginMaster.Plugins);

            client.StartClient();

            while (true) { }
        }
    }
}