
public class PluginManager
{
	private static PluginExecutor executor = new PluginExecutor();

	public static string Run(Plugin plugin) 
	{
		switch(plugin.Type)
		{
			case Plugin.FileType.DLL:
				return executor.runFromDLL(plugin);
			case Plugin.FileType.SCRIPT:
				return executor.runFromScript(plugin);
		}
		return null;	
	}
}
