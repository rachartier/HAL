using HAL.OSData;
using HAL.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        }

        /*
         * A plugin's configuration is composed of 3 majors attributes:
         *  - hearthbeat (in hours), the plugin will be launched one time per heartbeat.
         *  - activated (bool), tell if the plugin is activated or not
         *  - os (array), is composed of one or more familly os (linux, windows, osx)
         *      plugin will be executed only on the specified OS
         *
         */
        public override void SetPluginConfiguration(PluginFile plugin)
        {
            if (Root == null)
                return;

            JObject pluginConfig = Root["plugins"].Value<JObject>(plugin.FileName);

            // plugin needs to have a specific configuration, otherwise it can't be run
            if (pluginConfig == null)
            {
                throw new NullReferenceException($"Plugin {plugin.FileName} does not have any configuration.");
            }

            plugin.Hearthbeat = pluginConfig["hearthbeat"].Value<double>();
            plugin.Activated = pluginConfig["activated"].Value<bool>();

            // if no os is specified then all of them is authorized
            if (pluginConfig["os"] == null)
            {
                plugin.OsAuthorized |= OSAttribute.TargetFlag.All;
            }
            else
            {
                foreach (var os in pluginConfig["os"].ToObject<string[]>())
                {
                    if (!OSAttribute.OSNameToTargetFlag.ContainsKey(os))
                        throw new ArgumentException($"OS {os} is not recognized.");

                    plugin.OsAuthorized |= OSAttribute.OSNameToTargetFlag[os];
                }
            }
        }
    }
}