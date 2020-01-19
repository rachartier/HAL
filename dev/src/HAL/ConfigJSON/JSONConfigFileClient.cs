using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.MagicString;
using HAL.OSData;
using HAL.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HAL.Configuration
{
    public class JSONConfigFileClient : IConfigFileClient<JObject, JToken>
    {
        private static readonly int DEFAULT_PORT = 11000;

        public JSONConfigFileClient()
        {
        }

        public override void Load(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"'{file} not found.'");
            }

            string jsonData = File.ReadAllText(file);

            // root is composed of all the leaves
            Root = JObject.Parse(jsonData);
        }

        /*
         * A plugin's configuration is composed of 3 majors attributes:
         *  - hearthbeat (in hours), the plugin will be launched one time per heartbeat.
         *  - activated (bool), tell if the plugin is activated or not
         *  - os (array), is composed of one or more familly os (linux, windows, osx)
         *      plugin will be executed only on the specified OS
         *
         */

        public override void SetPluginConfiguration(APlugin plugin)
        {
            if (Root == null)
            {
                return;
            }

            JObject pluginConfig = Root[MagicStringEnumerator.JSONPlugins].Value<JObject>(plugin.Infos.FileName);

            T _SetAttribute<T>(string name, T fallback) where T : struct
            {
                T? val = pluginConfig[name]?.Value<T?>();

                if (val == null)
                {
                    Log.Instance?.Warn($"{plugin.Infos.FileName} has not \"{name}\" set. Default value used \"{fallback}\".");
                    return fallback;
                }

                return val.Value;
            }

            // plugin needs to have a specific configuration, otherwise it can't be run
            if (pluginConfig == null)
            {
                throw new NullReferenceException($"Plugin {plugin.Infos.FileName} does not have any configuration.");
            }

            plugin.Heartbeat = _SetAttribute(MagicStringEnumerator.JSONHeartbeat, 1.0f);
            plugin.Activated = _SetAttribute(MagicStringEnumerator.JSONActivated, false);

            if (OSAttribute.IsLinux)
            {
                bool needAdministratorRights = _SetAttribute(MagicStringEnumerator.JSONAdminRights, false);

                if (needAdministratorRights)
                {
                    string administratorUsername = pluginConfig[MagicStringEnumerator.JSONAdminUsername]?.Value<string>();

                    if (string.IsNullOrEmpty(administratorUsername))
                    {
                        throw new ArgumentException($"Adminstrator username must be specified in {plugin.Infos.FileName}'.");
                    }

                    plugin.AdministratorUsername = administratorUsername;
                }
            }

            // if no os is specified then all of them are authorized
            if (pluginConfig[MagicStringEnumerator.JSONOs] == null)
            {
                plugin.OsAuthorized |= OSAttribute.TargetFlag.All;
            }
            else
            {
                foreach (var os in pluginConfig[MagicStringEnumerator.JSONOs].ToObject<string[]>())
                {
                    if (!OSAttribute.OSNameToTargetFlag.ContainsKey(os))
                    {
                        throw new ArgumentException($"OS {os} is not recognized.");
                    }

                    plugin.OsAuthorized |= OSAttribute.OSNameToTargetFlag[os];
                }
            }

            if (pluginConfig[MagicStringEnumerator.JSONDifferencialAll]?.Value<bool>() == true)
            {
                plugin.ObserveAllAttributes = true;
            }
            else
            {
                var attributesToObserve = GetAttributesToObserve(pluginConfig);
                if (attributesToObserve != null)
                {
                    plugin.AttributesToObserve = attributesToObserve;
                }
            }
        }

        public override int GetPort()
        {
            if (Root == null)
            {
                return DEFAULT_PORT;
            }

            int? port = Root[MagicStringEnumerator.JSONServer]?.Value<int?>(MagicStringEnumerator.JSONPort);

            if (port == null)
                return DEFAULT_PORT;

            return port.Value;
        }

        public override string GetAddress()
        {
            if (Root == null)
            {
                return null;
            }

            string address = Root[MagicStringEnumerator.JSONServer]?.Value<string>(MagicStringEnumerator.JSONAddress);

            return address;
        }

        public override void SetScriptExtensionsConfiguration(IPluginMaster pluginMaster)
        {
            if (Root == null)
            {
                return;
            }

            try
            {
                JToken[] extensionsConfig = Root[MagicStringEnumerator.JSONCustomExtensions].Values<JToken>().ToArray();

                foreach (var ext in extensionsConfig)
                {
                    try
                    {
                        pluginMaster.AddScriptExtension((ext as JProperty).Name, ext.ToObject<string>());
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Instance?.Error(ex.Message);
                    }
                }
            }
            catch (NullReferenceException)
            {
                Log.Instance?.Warn("custom_extensions is not found in the configuration file.");
            }
        }

        public override void SetInterpreterNameConfiguration(IPluginMaster pluginMaster)
        {
            if (Root == null)
            {
                return;
            }

            var interpreterConfig = Root[MagicStringEnumerator.JSONIntepreter];

            foreach (var fileType in pluginMaster.AcceptedFilesTypes[PluginFileInfos.FileType.Script])
            {
                string key = fileType;
                string val = "";

                try
                {
                    // an intepreter can change depending the os
                    val = interpreterConfig[OSAttribute.GetOSFamillyName()]?.Value<string>(pluginMaster.ExtensionsNames[fileType]);
                }
                catch (NullReferenceException)
                {
                    // if we cant find one we need to search in the environment variables
                    var extensionName = pluginMaster.ExtensionsNames[fileType].ToUpper();

                    if (OSAttribute.IsWindows)
                    {
                        foreach (EnvironmentVariableTarget enumValue in Enum.GetValues(typeof(EnvironmentVariableTarget)))
                        {
                            val = Environment.GetEnvironmentVariable(extensionName, enumValue);

                            if (val != null)
                            {
                                Log.Instance?.Info($"Environment variable found: {extensionName} : {val}");
                                break;
                            }
                        }
                    }
                    else if (OSAttribute.IsLinux)
                    {
                        using var dllimport = new DllImportLaunchCmdUnix();

                        val = dllimport.UseLaunchCommand($"printenv | grep {extensionName} | cut -d '=' -f 2").Trim();

                        if (!string.IsNullOrEmpty(val))
                        {
                            Log.Instance?.Info($"Environment variable found: {extensionName} : {val}");
                        }
                    }
                }
                // if it can't be found, the default one is choose
                if (string.IsNullOrEmpty(val))
                {
                    val = pluginMaster.ExtensionsNames[fileType];
                }

                if (!File.Exists(val))
                {
                    return;
                }

                if (!pluginMaster.ExtensionsNames.ContainsKey(key))
                {
                    pluginMaster.AddScriptExtension(key, val);
                }
                else
                {
                    pluginMaster.ExtensionsNames[key] = val;
                }
            }
        }

        public override string GetDataBaseConnectionString()
        {
            if (Root == null)
            {
                return null;
            }

            string connectionString = Root[MagicStringEnumerator.JSONDatabase]?.Value<string>(MagicStringEnumerator.JSONConnectionString);

            return connectionString;
        }

        public override string GetStorageName()
        {
            if (Root == null)
            {
                return null;
            }

            string storageName = Root.Value<string>(MagicStringEnumerator.JSONStorageName);

            return storageName;
        }

        public override string GetSavePath()
        {
            if (Root == null)
            {
                return null;
            }

            string path = Root.Value<string>(MagicStringEnumerator.JSONSavePath);

            if (string.IsNullOrEmpty(path))
            {
                return "results/";
            }

            return path;
        }


        private List<string> GetAttributesToObserve(JObject pluginConfig)
        {
            var differencial = pluginConfig[MagicStringEnumerator.JSONDifferencial];

            if (differencial == null)
                return null;

            return differencial.ToObject<List<string>>();
        }
    }
}
