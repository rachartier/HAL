public class PluginManager
{
	private PluginExecutor executor = new PluginExecutor();

	public void Run(Plugin plugin, IStorage storage) 
	{
		switch(plugin.Type)
		{
			case Plugin.FileType.DLL:
				executor.runFromDLL(plugin, storage);
				break;
			case Plugin.FileType.SCRIPT:
				executor.runFromScript(plugin, storage);
				break;
		}
	}
}
