﻿using HAL.Executor;
using HAL.Executor.ThreadPoolExecutor;
using HAL.Loggin;
using HAL.Scheduler;
using System;

using System.Collections.Generic;

namespace HAL.Plugin.Mananger
{
    public class ScheduledPluginManager : APluginManager
    {
        public ScheduledPluginManager(IPluginMaster pluginMaster)
        {
            Executor = new ThreadPoolPluginExecutor(pluginMaster);
        }

        /// <summary>
        /// schedule a plugin to be executed at each heartbeat
        /// </summary>
        /// <param name="plugin">the plugin to be scheduled</param>
        public void SchedulePlugin(APlugin plugin)
        {
            if (!plugin.CanBeRun())
            {
                return;
            }

            try
            {
                // the scheduler is called to run the plugin each heartbeat
                SchedulerService.Instance.ScheduleTask($"task_{plugin.Infos.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
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
        /// schedule a list of plugins to be executed
        /// </summary>
        /// <param name="plugins">a collection of plugins</param>
        public void SchedulePlugins(IEnumerable<APlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                SchedulePlugin(plugin);
            }
        }
    }
}