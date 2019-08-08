using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

public class PluginManager
{
	private const string METHOD_RUN_NAME = "Run";

	public static string Run(Plugin plugin) 
	{
		switch(plugin.Type)
		{
			case Plugin.FileType.DLL:
				return runFromDLL(plugin);
			case Plugin.FileType.SCRIPT:
				return runFromScript(plugin);
		}
		return null;	
	}

	private static string runFromDLL(Plugin plugin)
	{
		var assembly = Assembly.LoadFrom(plugin.FilePath);
		var type = assembly.GetTypes().FirstOrDefault();

		if(type == null)
		{
			throw new NullReferenceException("Bad assembly type");
		}

		MethodInfo entryPointMethod = type.GetMethod(METHOD_RUN_NAME);

		dynamic instance = Activator.CreateInstance(type);
		dynamic result = entryPointMethod.Invoke(instance, null);

		return result.ToString();
	}

	private static string runFromScript(Plugin plugin) 
	{
		var start = new ProcessStartInfo();

		start.FileName = plugin.FilePath;
		start.UseShellExecute = false;
		start.CreateNoWindow = true;
		start.RedirectStandardOutput = true;
		start.RedirectStandardError = true;

		using (Process process = Process.Start(start))
		{
			using (StreamReader reader = process.StandardOutput)
			{
				return reader.ReadToEnd(); 
			}
		}
	}
}
