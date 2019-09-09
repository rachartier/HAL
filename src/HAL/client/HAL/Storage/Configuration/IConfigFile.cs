using HAL.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HAL.Storage.Configuration
{
    /// <summary>
    /// a configuration file is composed of a composite, root wil be the first to be read
    /// </summary>
    /// <typeparam name="TRoot">the root type</typeparam>
    /// <typeparam name="TToken">the token type</typeparam>
    public abstract class IConfigFile<TRoot, TToken> 
        where TRoot: class
        where TToken: class
    {
        public static TRoot Root { get; protected set; }

        /// <summary>
        /// load a configuration file
        /// </summary>
        /// <param name="file">configuration file path</param>
        public abstract void Load(string file);

        /// <summary>
        /// set a plugin's configuration from the configuration file
        /// </summary>
        /// <param name="plugin"></param>
        public abstract void SetPluginConfiguration(PluginFile plugin);

        /// <summary>
        /// set a list of plugins configuration
        /// </summary>
        /// <param name="plugins"></param>
        public void SetPluginsConfiguration(IEnumerable<PluginFile> plugins)
        {
            foreach (var plugin in plugins)
            {
                SetPluginConfiguration(plugin);
            }
        }
    }
}
