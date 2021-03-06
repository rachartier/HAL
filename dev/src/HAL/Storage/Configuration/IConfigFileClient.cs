﻿using System;
using System.Collections.Generic;
using HAL.Loggin;
using HAL.Plugin;

namespace HAL.Configuration
{
    public abstract class IConfigFileClient<TRoot, TToken> : IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
    {
        /// <summary>
        ///     set a plugin's configuration from the configuration file
        /// </summary>
        /// <param name="plugin"></param>
        public abstract void SetPluginConfiguration(APlugin plugin);

        /// <summary>
        ///     add all the custom extensions that a script can be
        ///     other plugin's type, like dll and so should not be modified to operate properly
        /// </summary>
        /// <param name="pluginMaster">the plugin master wich will be configured</param>
        public abstract void SetScriptExtensionsConfiguration(IPluginMaster pluginMaster);

        /// <summary>
        ///     set all the custom interpreters
        /// </summary>
        /// <param name="pluginMaster">the plugin master wich will be configured</param>
        public abstract void SetInterpreterNameConfiguration(IPluginMaster pluginMaster);

        /// <summary>
        ///     return one or more connection strings
        /// </summary>
        /// <returns>connections strings if it exists, null otherwise</returns>
        public abstract string[] GetDataBaseConnectionStrings();

        /// <summary>
        ///     return storage names
        /// </summary>
        /// <returns>return storage names or null otherwise</returns>
        public abstract string[] GetStorageNames();

        /// <summary>
        ///     Get the port of the socket server
        /// </summary>
        /// <returns>The port int if it exist, 11000 otherwise</returns>
        public abstract int GetPort();

        /// <summary>
        ///     Get the address wich will be running the server
        /// </summary>
        public abstract string GetAddress();

        /// <summary>
        ///     Get the path to store the plugin's results
        /// </summary>
        /// <returns>the path</returns>
        public abstract string GetSavePath();

        /// <summary>
        ///     set a list of plugins configuration
        /// </summary>
        /// <param name="plugins"></param>
        public void SetPluginsConfiguration(IEnumerable<APlugin> plugins)
        {
            foreach (var plugin in plugins)
                try
                {
                    SetPluginConfiguration(plugin);
                    plugin.AlreadyConfigured = true;
                }
                catch (Exception e)
                {
                    if (!plugin.AlreadyConfigured)
                        Log.Instance?.Error($"{e.Message} Plugin {plugin.Infos.FileName} ignored.");
                }
        }
    }
}