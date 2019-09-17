
using HAL.Loggin;
using HAL.Plugin.Executor;
using HAL.Plugin.Schelduler;
using System;
using System.Collections.Generic;

namespace HAL.Plugin.Mananger
{
    public class PluginManager
    {
        public readonly PluginExecutor Executor;

        public PluginManager(PluginMaster pluginMaster)
        {
            Executor = new PluginExecutor(pluginMaster);
        }

        /// <summary>
        /// run a plugin depending it's type
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void Run(BasePlugin plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                switch (plugin.Type)
                {
                    case BasePlugin.FileType.DLL:
                        Executor.RunFromDLL(plugin);
                        break;
                    case BasePlugin.FileType.Script:
                        Executor.RunFromScript(plugin);
                        break;
                    case BasePlugin.FileType.SharedObject:
                        Executor.RunFromSO(plugin);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message);
            }
        }

        /// <summary>
        /// scheldule a plugin to be executed at each heartbeat
        /// </summary>
        /// <param name="plugin">the plugin to be schelduled</param>
        public void ScheldulePlugin(BasePlugin plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                // the schelduler is called to run the plugin each heartbeat
                ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
                {
                    Run(plugin);
                    Log.Instance.Info($"{plugin.FileName} correctly executed.");
                });

            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message);
            }
        }

        /// <summary>
        /// scheldule a list of plugins to be executed
        /// </summary>
        /// <param name="plugins">a collection of plugins</param>
        public void ScheldulePlugins(IEnumerable<BasePlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                ScheldulePlugin(plugin);
            }
        }
    }
}