using HAL.Executor.ThreadPoolExecutor;
using HAL.Plugin;
using HAL.Plugin.Mananger;
using HAL.Storage.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace plugins_checker
{
    internal class Program
    {
        private static object key = new object();

        private static void Main(string[] args)
        {
            var checkerFile = new CheckerFileJSON();
            checkerFile.Load("config.json");

            foreach (var path in checkerFile.PathToCheck)
            {
                Console.WriteLine($"\nPath: {path}");

                var pluginMaster = new PluginMasterBasePlugin();
                var pluginManager = new SimplePluginManager(pluginMaster);

                IConfigFile<JObject, JToken> configFile = new JSONConfigFile();
                configFile.Load($"{path}/config/config.json");

                configFile.SetScriptExtensionsConfiguration(pluginMaster);
                configFile.SetInterpreterNameConfiguration(pluginMaster);

                foreach (var file in Directory.GetFiles($"{path}/plugins/"))
                {
                    pluginMaster.AddPlugin(file);
                }

                configFile.SetPluginsConfiguration(pluginMaster.Plugins);

                foreach (var plugin in pluginMaster.Plugins)
                {
                    plugin.OnExecutionFinished += new EventHandler<APlugin.PluginResultArgs>((o, e) =>
                    {
                        bool valid = IsValidJSON(e.Result, out string error);

                        lock (key)
                        {
                            PrintPluginLine(e.Plugin, valid, error);
                        }
                    });

                    pluginManager.Run(plugin);
                }

                (pluginManager.Executor as ThreadPoolPluginExecutor).WaitForEmptyPool();
                Console.WriteLine("-----------------------------------------");
            }
        }

        private static void PrintPluginLine(APlugin plugin, bool valid, string error)
        {
            Console.ForegroundColor = (valid) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"[{plugin.Infos.FileName}]:");
            Console.ResetColor();
            Console.WriteLine($" {error}");
        }

        private static bool IsValidJSON(string json, out string error)
        {
            try
            {
                var obj = JToken.Parse(json);
                error = "No error.";

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }
    }
}