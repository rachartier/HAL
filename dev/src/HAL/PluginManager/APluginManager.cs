﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HAL.Executor;

namespace HAL.Plugin.Mananger
{
    public abstract class APluginManager
    {
        public IPluginExecutor Executor { get; protected set; }

        /// <summary>
        ///     run a plugin depending it's type
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void Run(APlugin plugin)
        {
            if (!plugin.CanBeRun()) return;

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

        public virtual void SchedulePlugin(APlugin plugin)
        {
        }

        public virtual void SchedulePlugins(IEnumerable<APlugin> plugins)
        {
        }

        public virtual void UnscheduleAllPlugins(IEnumerable<APlugin> plugins)
        {
        }

        public virtual Task UnscheduleAllPluginsAsync(IEnumerable<APlugin> plugins)
        {
            return null;
        }
    }
}