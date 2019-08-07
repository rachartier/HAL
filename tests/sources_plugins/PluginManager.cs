using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

public class PluginManager
{
	private const string METHOD_RUN_NAME = "Run";
	private const string CLASS_NAME = "Plugin";

	public static string Run(Plugin plugin) 
	{
		if(plugin.Type == Plugin.FileType.DLL)
		{
			return runFromDLL(plugin);
		}
		else if(plugin.Type == Plugin.FileType.SCRIPT)
		{
			return runFromScript(plugin);
		}
		return null;	
	}

	private static string runFromDLL(Plugin plugin)
	{
		var assembly = Assembly.LoadFrom(plugin.FilePath);
		var type = assembly.GetType($"{plugin.Name}.{CLASS_NAME}");

		MethodInfo[] methods = type.GetMethods();

		dynamic instance = Activator.CreateInstance(type);
		object result = null;

		foreach(MethodInfo method in methods)
		{
			if(method.Name.Equals(METHOD_RUN_NAME))
			{
				result = method.Invoke(instance, null);
			}
		}

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
