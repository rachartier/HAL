
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
#if __LINUX || __OSX
            case Plugin.FileType.SharedObject:
                Executor.RunFromSO(plugin, storage);
                break;
#endif
        }
    }

    public void ScheldulePlugin(Plugin plugin, IStorage storage)
    {
        if (!canBeRun(plugin))
            return;

        ScheldulerService.Instance.SchelduleTask($"task_{plugin.FileName}", plugin.Hearthbeat, () =>
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

        if (!(((plugin.OsAuthorized & OSTarget.Linux) != 0) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || ((plugin.OsAuthorized & OSTarget.Windows) != 0) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        || ((plugin.OsAuthorized & OSTarget.OSX) != 0) && RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
        {
            return false;
        }

        return true;
    }
}
