using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

namespace HAL
{
    public class Program
    {
        private static HalClient client;
        private static readonly List<IStoragePlugin> storages = new List<IStoragePlugin>();
        private static bool receiveError;

        private static void CleanExit()
        {
            foreach (var storage in storages)
                storage?.Dispose();
            client?.Disconnect();
            client?.Dispose();

            if (receiveError)
            {
                Log.Instance?.Error("Program closed due to errors.");
                return;
            }

            Log.Instance?.Error("Unexcepted program exit.");

            Environment.Exit(0);
        }

        private static void Main(string[] args)
        {
            IConfigFileClient<JObject, JToken> configFileLocal = new JSONConfigFileClient();

            try
            {
                configFileLocal.Load(MagicStringEnumerator.DefaultLocalConfigPath);
                Log.Instance?.Info($"Configuration file {MagicStringEnumerator.DefaultLocalConfigPath} loaded.");
            }
            catch (Exception ex)
            {
                Log.Instance?.Error($"{MagicStringEnumerator.DefaultLocalConfigPath}: {ex.Message}");
                CleanExit();
            }

            var ip = configFileLocal.GetAddress();
            var port = configFileLocal.GetPort();

            Log.Instance?.Info($"Server ip: {ip}");
            Log.Instance?.Info($"Server port: {port}");

            AppDomain.CurrentDomain.ProcessExit += (o, e) => { CleanExit(); };

            client = new HalClient(ip, port);
            new Thread(async () => { await client.StartAsync(); }).Start();

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
                    Log.Instance?.Info($"Configuration file {MagicStringEnumerator.DefaultConfigPath} loaded.");
                }
                catch (Exception ex)
                {
                    Log.Instance?.Error($"{MagicStringEnumerator.DefaultConfigPath}: {ex.Message}");
                    receiveError = true;
                    CleanExit();
                }

                foreach (var storageName in configFile.GetStorageNames())
                {
                    var storage = StorageFactory.CreateStorage(storageName);

                    if (storage is StorageServerFile)
                        (storage as StorageServerFile).StreamWriter = client.StreamWriter;
                    else if (storage is StorageLocalFile)
                        (storage as StorageLocalFile).SavePath = configFile.GetSavePath();

                    storages.Add(storage);
                    Log.Instance?.Info($"Storage \"{storageName}\" added.");
                }

                await pluginManager.UnscheduleAllPluginsAsync(pluginMaster.Plugins);

                pluginMaster.RemoveAllPlugins();

                /*
                * A connection string is needed if you want to access a database
                *
                * if none is specified, then it will do nothing and return null.
                */

                var connectionStrings = configFile.GetDataBaseConnectionStrings();
                var databases = storages.Where(s => s is IDatabaseStorage).ToArray();

                for (var i = 0; i < connectionStrings?.Length; ++i)
                {
                    var db = databases[i];
                    var connectionString = connectionStrings[i];

                    db.Init(connectionString);
                    Log.Instance?.Info($"Database \"{db}\" with connection string \"{connectionString}\"");
                }

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
                    pluginMaster.AddPlugin(file);

                /*
                 * Then the configuration of all of the plugins is set.
                 */
                configFile.SetPluginsConfiguration(pluginMaster.Plugins);
                configFileLocal.SetPluginsConfiguration(pluginMaster.Plugins);
                configFileLocal.SetInterpreterNameConfiguration(pluginMaster);

                /*
                 * An event is added when the plugin's execution is finished to save it where the user specified above.
                 */
                var savePath = configFile.GetSavePath();
                foreach (var plugin in pluginMaster.Plugins)
                    plugin.OnExecutionFinished += async (o, e) =>
                    {
                        foreach (var storage in storages)
                            try
                            {
                                var code = await storage.Save(e.Plugin, e.Result);

                                if (code == StorageCode.Failed) Log.Instance?.Error("Storage failed.");
                            }
                            catch (Exception ex)
                            {
                                Log.Instance?.Error($"Storage failed: {ex.Message}");
                            }
                    };

                /*
                 * All the plugins are then schelduled to be launched when needed.
                 */
                pluginManager.SchedulePlugins(pluginMaster.Plugins);

                Log.Instance?.Info("Configuration reloaded.");
            };

            while (!receiveError) Thread.Sleep(100);

            CleanExit();
        }
    }
}