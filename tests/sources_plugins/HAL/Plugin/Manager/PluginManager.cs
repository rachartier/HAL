
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

        public void Run(PluginFile plugin, IStorage storage)
        {
            if (!canBeRun(plugin))
                return;

            switch (plugin.Type)
            {
                case PluginFile.FileType.DLL:
                    Executor.RunFromDLL(plugin, storage);
                    break;
                case PluginFile.FileType.Script:
                    Executor.RunFromScript(plugin, storage);
                    break;
                case PluginFile.FileType.SharedObject:
                    Executor.RunFromSO(plugin, storage);
                    break;
            }
        }

        public void ScheldulePlugin(PluginFile plugin, IStorage storage)
        {
            if (!canBeRun(plugin))
                return;

            ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
            {
                Run(plugin, storage);
            });
        }

        public void ScheldulePlugins(IEnumerable<PluginFile> plugins, IStorage storage)
        {
            foreach (var plugin in plugins)
            {
                ScheldulePlugin(plugin, storage);
            }
        }

        private bool canBeRun(PluginFile plugin)
        {
            if (plugin.Activated == false)
                return false;

            if (!(((plugin.OsAuthorized & OSAttribute.TargetFlag.Linux) != 0) && OSAttribute.IsLinux
            || ((plugin.OsAuthorized & OSAttribute.TargetFlag.Windows) != 0) && OSAttribute.IsWindows
            || ((plugin.OsAuthorized & OSAttribute.TargetFlag.OSX) != 0) && OSAttribute.IsOSX))
            {
                return false;
            }

            return true;
        }
    }
}