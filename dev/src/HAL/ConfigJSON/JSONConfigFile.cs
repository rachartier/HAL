using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.MagicString;
using HAL.OSData;
using HAL.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace HAL.Storage.Configuration
{
    public class JSONConfigFile : IConfigFile<JObject, JToken>
    {
        public JSONConfigFile()
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

            Log.Instance?.Info($"Configuration file {file} loaded");
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

            void _SetAttribute<T>(out T attr, string name, T fallback) where T : struct
            {
                T? val = pluginConfig[name]?.Value<T?>();

                if (val == null)
                {
                    Log.Instance?.Error($"{plugin.Infos.FileName} has not \"{name}\" set. Default value used \"{fallback}\".");
                    attr = fallback;
                }
                else
                {
                    attr = val.Value;
                }
            }

            // plugin needs to have a specific configuration, otherwise it can't be run
            if (pluginConfig == null)
            {
                throw new NullReferenceException($"Plugin {plugin.Infos.FileName} does not have any configuration.");
            }

            _SetAttribute(out double heartbeat, MagicStringEnumerator.JSONHeartbeat, 1);
            _SetAttribute(out bool activated, MagicStringEnumerator.JSONActivated, false);

            plugin.Heartbeat = heartbeat;
            plugin.Activated = activated;

            if (OSAttribute.IsLinux)
            {
                bool? needAdministratorRights = pluginConfig[MagicStringEnumerator.JSONAdminRights]?.Value<bool>();
                plugin.AdministratorRights = needAdministratorRights.GetValueOrDefault();

                if (needAdministratorRights == true)
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

                        if (string.IsNullOrEmpty(val))
                        {
                            Log.Instance?.Info($"Environment variable not found: {extensionName}");
                        }
                        else
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

                if(!File.Exists(val))
                    return;

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
    }
}
