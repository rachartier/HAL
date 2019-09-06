using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class JSONConfigFile
{
    public static JObject Root { get; private set; }

    private static Dictionary<string, OSTarget> osMask = new Dictionary<string, OSTarget>()
    {
        ["linux"] = OSTarget.Linux,
        ["windows"] = OSTarget.Windows,
        ["macos"] = OSTarget.OSX
    };

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
            plugin.OsAuthorized |= OSTarget.Linux;
            plugin.OsAuthorized |= OSTarget.Windows;
            plugin.OsAuthorized |= OSTarget.OSX;
        }
        else
        {
            foreach (var os in pluginConfig["os"].ToObject<string[]>())
            {
                if (!osMask.ContainsKey(os))
                    throw new ArgumentException($"OS {os} is not recognized.");

                plugin.OsAuthorized |= osMask[os];
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

