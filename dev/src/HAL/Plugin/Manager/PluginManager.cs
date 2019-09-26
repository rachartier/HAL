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

        public PluginManager(IPluginMaster pluginMaster)
        {
            Executor = new PluginExecutor(pluginMaster);
        }

        /// <summary>
        /// run a plugin depending it's type
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void Run(APlugin plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                switch (plugin.Type)
                {
                    case PluginFileInfos.FileType.DLL:
                        Executor.RunFromDLL(plugin);
                        break;
                    case PluginFileInfos.FileType.Script:
                        Executor.RunFromScript(plugin);
                        break;
                    case PluginFileInfos.FileType.SharedObject:
                        Executor.RunFromSO(plugin);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Instance?.Error(e.Message);
            }
        }

        /// <summary>
        /// scheldule a plugin to be executed at each heartbeat
        /// </summary>
        /// <param name="plugin">the plugin to be schelduled</param>
        public void ScheldulePlugin(APlugin plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                // the schelduler is called to run the plugin each heartbeat
                ScheldulerService.Instance.SchelduleTask($"task_{plugin.Infos.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
                {
                    Run(plugin);
                    Log.Instance?.Info($"{plugin.Infos.FileName} correctly executed.");
                });
            }
            catch (Exception e)
            {
                Log.Instance?.Error(e.Message);
            }
        }

        /// <summary>
        /// scheldule a list of plugins to be executed
        /// </summary>
        /// <param name="plugins">a collection of plugins</param>
        public void ScheldulePlugins(IEnumerable<APlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                ScheldulePlugin(plugin);
            }
        }
    }
}
