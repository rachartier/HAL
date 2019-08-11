public class PluginManager
{
    public PluginExecutor Executor { get; private set; } = new PluginExecutor();

    public void Run(Plugin plugin, IStorage storage)
    {
        switch (plugin.Type)
        {
            case Plugin.FileType.DLL:
                Executor.RunFromDLL(plugin, storage);
                break;
            case Plugin.FileType.Script:
                Executor.RunFromScript(plugin, storage);
                break;
        }
    }
}
