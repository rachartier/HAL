
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class PluginManager
{
    public PluginExecutor Executor { get; private set; } = new PluginExecutor();

    public void Run(Plugin plugin, IStorage storage)
    {
        if (!canBeRun(plugin))
            return;

        switch (plugin.Type)
        {
            case Plugin.FileType.DLL:
                Executor.RunFromDLL(plugin, storage);
                break;
            case Plugin.FileType.Script:
                Executor.RunFromScript(plugin, storage);
                break;
            case Plugin.FileType.SharedObject:
                Executor.RunFromSO(plugin, storage);
                break;
        }
    }

    public void ScheldulePlugin(Plugin plugin, IStorage storage)
    {
        if (!canBeRun(plugin))
            return;

        ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}_{Guid.NewGuid()}", plugin.Hearthbeat, () =>
        {
            Run(plugin, storage);
        });
    }

    public void ScheldulePlugins(IEnumerable<Plugin> plugins, IStorage storage)
    {
        foreach (var plugin in plugins)
        {
            ScheldulePlugin(plugin, storage);
        }
    }

    private bool canBeRun(Plugin plugin)
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
