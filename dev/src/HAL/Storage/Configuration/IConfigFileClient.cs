using HAL.Loggin;
using HAL.Plugin;
using System;
using System.Collections.Generic;

namespace HAL.Configuration
{
    public abstract class IConfigFileClient<TRoot, TToken> : IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
    {
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
        /// return the connection string if one is present
        /// </summary>
        /// <returns>the connection string if it exists, null otherwise</returns>
        public abstract string GetDataBaseConnectionString();

        /// <summary>
        /// return the storage name
        /// </summary>
        /// <returns>return storage name or null otherwise</returns>
        public abstract string GetStorageName();

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