using HAL.Plugin;
using HAL.Loggin;
using System.Collections.Generic;
using System;

namespace HAL.Storage.Configuration
{
    /// <summary>
    /// a configuration file is composed of a composite, root wil be the first to be read
    /// </summary>
    /// <typeparam name="TRoot">the root type</typeparam>
    /// <typeparam name="TToken">the token type</typeparam>
    public abstract class IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
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
        public abstract void SetPluginConfiguration(APlugin plugin);

        /// <summary>
        /// add all the custom extensions that a script can be
        /// other plugin's type, like dll and so should not be modified to operate properly
        /// </summary>
        /// <param name="pluginMaster">the plugin master wich will be configured</param>
        public abstract void SetScriptExtensionsConfiguration(IPluginMaster pluginMaster);

        /// <summary>
        /// set all the custom interpreters
        /// </summary>
        /// <param name="pluginMaster">the plugin master wich will be configured</param>
        public abstract void SetInterpreterNameConfiguration(IPluginMaster pluginMaster);

        /// <summary>
        /// set a list of plugins configuration
        /// </summary>
        /// <param name="plugins"></param>
        public void SetPluginsConfiguration(IEnumerable<APlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    SetPluginConfiguration(plugin);
                }
                catch (Exception e)
                {
                    Log.Instance?.Error($"{e.Message} Plugin ignored.");
                }
            }
        }
    }
}