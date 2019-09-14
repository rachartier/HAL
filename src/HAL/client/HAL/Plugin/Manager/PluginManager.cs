
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
        public void Run(PluginFile plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                switch (plugin.Type)
                {
                    case PluginMaster.FileType.DLL:
                        Executor.RunFromDLL(plugin);
                        break;
                    case PluginMaster.FileType.Script:
                        Executor.RunFromScript(plugin);
                        break;
                    case PluginMaster.FileType.SharedObject:
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
        /// <param name="plugin"></param>
        public bool ScheldulePlugin(PluginFile plugin)
        {
            if (!plugin.CanBeRun())
            {
                return false;
            }

            try
            {
                // the schelduler is called to run the plugin each heartbeat
                ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
                {
                    Run(plugin);
                    Log.Instance.Info($"{plugin.FileName} correctly executed.");
                });

                return true;
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// scheldule a list of plugins to be executed
        /// </summary>
        /// <param name="plugins"></param>
        public void ScheldulePlugins(IEnumerable<PluginFile> plugins)
        {
            foreach (var plugin in plugins)
            {
                ScheldulePlugin(plugin);
            }
        }
    }
}