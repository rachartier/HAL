using HAL.Loggin;
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

            JObject pluginConfig = Root["plugins"].Value<JObject>(plugin.Infos.FileName);

            // plugin needs to have a specific configuration, otherwise it can't be run
            if (pluginConfig == null)
            {
                throw new NullReferenceException($"Plugin {plugin.Infos.FileName} does not have any configuration.");
            }

            plugin.Hearthbeat = pluginConfig["hearthbeat"].Value<double>();
            plugin.Activated = pluginConfig["activated"].Value<bool>();

            if (OSAttribute.IsLinux)
            {
                bool? needAdministratorRights = pluginConfig["admin_rights"]?.Value<bool>();
                plugin.AdministratorRights = needAdministratorRights.GetValueOrDefault();
            }

            // if no os is specified then all of them are authorized
            if (pluginConfig["os"] == null)
            {
                plugin.OsAuthorized |= OSAttribute.TargetFlag.All;
            }
            else
            {
                foreach (var os in pluginConfig["os"].ToObject<string[]>())
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
                JToken[] extensionsConfig = Root["custom_extensions"].Values<JToken>().ToArray();

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
            foreach (var fileType in pluginMaster.AcceptedFilesTypes[PluginFileInfos.FileType.Script])
            {
                string key = fileType;
                string val = "";

                // an interpreter is needed to interpret the code
                var interpreterConfig = Root["interpreter"];

                if (interpreterConfig == null)
                {
                    Log.Instance?.Error("intepreter is not set in the configuration file.");
                    throw new NullReferenceException("intepreter is not set in the configuration file.");
                }

                // an intepreter can change depending the os
                val = interpreterConfig[OSAttribute.GetOSFamillyName()].Value<string>(pluginMaster.ExtensionsNames[fileType]);

                // if it can't be found, the default one is choose
                if (string.IsNullOrEmpty(val))
                {
                    val = pluginMaster.ExtensionsNames[fileType];
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
    }
}
