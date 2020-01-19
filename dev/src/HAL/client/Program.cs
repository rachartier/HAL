using HAL.Configuration;
using HAL.Connection.Client;
using HAL.Factory;
using HAL.Loggin;
using HAL.MagicString;
using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage;
using Newtonsoft.Json.Linq;
using Plugin.Manager;
using System;
using System.IO;
using System.Threading;

namespace HAL
{
    public class Program
    {
        private static HalClient client;
        private static IStoragePlugin storage;
        private static bool receiveError;

        private static void CleanExit()
        {
            storage?.Dispose();
            client.Disconnect();
            client.Dispose();

            if (receiveError)
            {
                Log.Instance?.Error("Program closed due to errors.");
                return;
            }
            else
            {
                Log.Instance?.Error("Unexcepted program exit.");
            }
        }
        private static void Main(string[] args)
        {
            IConfigFileClient<JObject, JToken> configFileLocal = new JSONConfigFileClient();
            try
            {
                configFileLocal.Load(MagicStringEnumerator.DefaultLocalConfigPath);
                Log.Instance?.Info($"Configuration file {MagicStringEnumerator.DefaultLocalConfigPath} loaded");
            }
            catch (Exception ex)
            {
                Log.Instance?.Error($"{MagicStringEnumerator.DefaultLocalConfigPath}: {ex.Message}");
                return;
            }

            /*
            * A storage is needed to save the output of the plugins
            *
            * the only purpose of text storage is to debug and do a showcase.
            *
            * you can switch on local file storage (wich will stock all the outputs on the client side)
            * or by mongodb stockage.
            */

            string ip = configFileLocal.GetAddress();
            int port = configFileLocal.GetPort();

            Log.Instance?.Info($"Server ip: {ip}");
            Log.Instance?.Info($"Server port: {port}");

            client = new HalClient(ip, port);
            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                CleanExit();
            };

            new Thread(async () =>
            {
                await client.StartAsync();
            }).Start();

            IPluginMaster pluginMaster = new PluginMasterBasePlugin();

            /*
             * The plugin manager will manage and schedule plugins
             *
             * when a plugin need to be executed, PluginManager will launch what the plugin needs and output the result
             * where desired above.
             */
            APluginManager pluginManager = new ScheduledPluginManager(pluginMaster);


            // We only want to configure all the plugins when the client has received all the informations and plugins
            client.OnReceiveDone += async (o, e) =>
            {
                /*
                * Here we instanciate the configuration file
                * Its a JSON format file.
                *
                * All HAL's client configuration is here.
                */
                IConfigFileClient<JObject, JToken> configFile = new JSONConfigFileClient();

                try
                {
                    configFile.Load(MagicStringEnumerator.DefaultConfigPath);
                    Log.Instance?.Info($"Configuration file {MagicStringEnumerator.DefaultConfigPath} loaded");
                }
                catch (Exception ex)
                {
                    Log.Instance?.Error($"{MagicStringEnumerator.DefaultConfigPath}: {ex.Message}");
                    receiveError = true;
                    return;
                }

                storage = StorageFactory.CreateStorage(configFile.GetStorageName());

                if (storage is StorageServerFile)
                {
                    (storage as StorageServerFile).StreamWriter = client.StreamWriter;
                }
                else if (storage is StorageLocalFile)
                {
                    (storage as StorageLocalFile).SavePath = configFile.GetSavePath();
                }

                // TODO: fix that
                // if this method isnt here twice, then its possible to the schelduled plugin not to be deleted
                // why? I dont know yet
                // update: if the heartbeat is to low, then it need to be called 3 times or more
                await pluginManager.UnscheduleAllPluginsAsync(pluginMaster.Plugins);
                await pluginManager.UnscheduleAllPluginsAsync(pluginMaster.Plugins);

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
                foreach (var file in Directory.GetFiles(MagicStringEnumerator.DefaultPluginPath))
                {
                    pluginMaster.AddPlugin(file);
                }

                /*
                 * Then the configuration of all of the plugins is set.
                 */
                configFile.SetPluginsConfiguration(pluginMaster.Plugins);
                configFileLocal.SetPluginsConfiguration(pluginMaster.Plugins);

                /*
                 * An event is added when the plugin's execution is finished to save it where the user specified above.
                 */
                var savePath = configFile.GetSavePath();
                foreach (var plugin in pluginMaster.Plugins)
                {
                    plugin.OnExecutionFinished += async (o, e) =>
                    {
                        try
                        {
                            var code = await storage.Save(e.Plugin, e.Result);

                            if (code == StorageCode.Failed)
                            {
                                Log.Instance?.Error("Storage failed.");
                            }

                        }
                        catch (Exception ex)
                        {
                            Log.Instance?.Error(ex.Message);
                            Log.Instance?.Error(ex.StackTrace);
                            Log.Instance?.Error("Storage failed.");
                        }
                    };
                }

                /*
                 * All the plugins are then schelduled to be launched when needed.
                 */
                pluginManager.SchedulePlugins(pluginMaster.Plugins);

                Log.Instance?.Info("Configuration reloaded.");
            };

            while (!receiveError) { Thread.Sleep(100); }

            CleanExit();
        }
    }
}
