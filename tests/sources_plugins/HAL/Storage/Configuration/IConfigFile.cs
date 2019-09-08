using HAL.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HAL.Storage.Configuration
{
    public abstract class IConfigFile<TRoot, TToken> 
        where TRoot: class
        where TToken: class
    {
        public static TRoot Root { get; protected set; }

        public abstract void Load(string file);
        public abstract void SetPluginConfiguration(PluginFile plugin);

        public void SetPluginsConfiguration(IEnumerable<PluginFile> plugins)
        {
            foreach (var plugin in plugins)
            {
                SetPluginConfiguration(plugin);
            }
        }
    }
}
