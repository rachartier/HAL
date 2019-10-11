using HAL.Executor;
using HAL.Loggin;
using System;
using System.Collections.Generic;

namespace HAL.Plugin.Mananger
{
    public abstract class APluginManager
    {
        public IPluginExecutor Executor { get; protected set; }

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
                Log.Instance?.Error($"{plugin.Infos.FileName} encountered a problem: {e.Message}");
            }
        }

        public virtual void SchedulePlugin(APlugin plugin)
        {
        }

        public virtual void SchedulePlugins(IEnumerable<APlugin> plugins)
        {
        }
    }
}