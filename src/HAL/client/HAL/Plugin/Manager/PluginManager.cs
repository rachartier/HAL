
using HAL.OSData;
using HAL.Plugin.Executor;
using HAL.Plugin.Schelduler;
using HAL.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HAL.Plugin.Mananger
{
    public class PluginManager
    {
        public PluginExecutor Executor { get; private set; } = new PluginExecutor();

        /// <summary>
        /// run a plugin depending it's type
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void Run(PluginFile plugin)
        {
            if (!plugin.CanBeRun())
                return;

            switch (plugin.Type)
            {
                case PluginFile.FileType.DLL:
                    Executor.RunFromDLL(plugin);
                    break;
                case PluginFile.FileType.Script:
                    Executor.RunFromScript(plugin);
                    break;
                case PluginFile.FileType.SharedObject:
                    Executor.RunFromSO(plugin);
                    break;
            }
        }

        /// <summary>
        /// scheldule a plugin to be executed at each heartbeat
        /// </summary>
        /// <param name="plugin"></param>
        public void ScheldulePlugin(PluginFile plugin)
        {
            if (!plugin.CanBeRun())
                return;

            // the schelduler is called to run the plugin each heartbeat
            ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
            {
                Run(plugin);
            });
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