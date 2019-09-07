using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class JSONConfigFile
{
    public static JObject Root { get; private set; }

    public void Load(string file)
    {
        if (!File.Exists(file))
        {
			throw new FileNotFoundException($"'{file} not found.'");
        }

        string jsonData = File.ReadAllText(file);
        Root = JObject.Parse(jsonData);
    }

    public void SetPluginConfiguration(Plugin plugin)
    {
        if (Root == null)
            return;

        JObject pluginConfig = Root["plugins"].Value<JObject>(plugin.FileName);

        if(pluginConfig == null)
        {
            throw new NullReferenceException($"Plugin {plugin.FileName} does not have any configuration.");
        }

        plugin.Hearthbeat = pluginConfig["hearthbeat"].Value<double>();
        plugin.Activated = pluginConfig["activated"].Value<bool>();

        if(pluginConfig["os"] == null)
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

    public void SetPluginsConfiguration(IEnumerable<Plugin> plugins)
    {
        foreach(var plugin in plugins)
        {
            SetPluginConfiguration(plugin);
        }
    }
}

