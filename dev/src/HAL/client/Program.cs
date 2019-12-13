using System;
using System.IO;
using System.Threading;
using HAL.Configuration;
using HAL.Connection.Client;
using HAL.Factory;
using HAL.Loggin;
using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage;
using Newtonsoft.Json.Linq;
using Plugin.Manager;

namespace HAL
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using HalClient client = new HalClient("127.0.0.1", 1664);
            
            AppDomain.CurrentDomain.ProcessExit += (o,e) => {
                client.Disconnect();
                client.Dispose();
                Log.Instance?.Error("Unexecpted program exit.");
            };

            new Thread(async () => {
                await client.StartAsync();
            }).Start();

            bool hasLocalConfigFile = false;
            
            IConfigFileClient<JObject, JToken> configFileLocal = new JSONConfigFileClient();
            
            try 
            {
                configFileLocal.Load("config/config_local.json");
                hasLocalConfigFile = true;
            }
            catch
            {
                Log.Instance?.Warn("Local config file not found. Ignored.");
                hasLocalConfigFile = false;
            }
            IPluginMaster pluginMaster = new PluginMasterBasePlugin();

            /*
             * The plugin manager will manage and schedule plugins
             *
             * when a plugin need to be executed, PluginManager will launch what the plugin needs and output the result
             * where desired above.
             */
            APluginManager pluginManager = new ScheduledPluginManager(pluginMaster);

            // We only want to configure all the plugins when the client has received all the informations and plugins
            client.OnReceiveDone += (o, e) =>
            {
                /*
                * Here we instanciate the configuration file
                * Its a JSON format file.
                *
                * All HAL's client configuration is here.
                */
                IConfigFileClient<JObject, JToken> configFile = new JSONConfigFileClient();
                configFile.Load("config/config.json");

                /*
                * A storage is needed to save the output of the plugins
                *
                * the only purpose of text storage is to debug and do a showcase.
                *
                * you can switch on local file storage (wich will stock all the outputs on the client side)
                * or by mongodb stockage.
                */
                IStoragePlugin storage = StorageFactory.CreateStorage(configFile.GetStorageName());

                pluginManager.UnscheduleAllPlugins(pluginMaster.Plugins);
                pluginMaster.RemoveAllPlugins();
                
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

                if(hasLocalConfigFile) 
                    configFileLocal.SetPluginsConfiguration(pluginMaster.Plugins);

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

                Log.Instance?.Info("Configuration reloaded.");
            };
            var loopThread = new Thread(() => {
                while (true) { Thread.Sleep(100); }
            });

            loopThread.Start();
            loopThread.Join();
        }
    }
}
