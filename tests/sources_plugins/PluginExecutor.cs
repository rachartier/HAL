using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

public class PluginExecutor 
{
	public string MethodEntryPointName {get;set;} = "Run";

	public string runFromDLL(Plugin plugin)
	{
		var assembly = Assembly.LoadFrom(plugin.FilePath);
		var type = assembly.GetTypes().FirstOrDefault();

		if(type == null)
		{
			throw new NullReferenceException("Bad assembly type.");
		}

		MethodInfo entryPointMethod = type.GetMethod(MethodEntryPointName);

		dynamic instance = Activator.CreateInstance(type);
		dynamic result = entryPointMethod.Invoke(instance, null);

		return result.ToString();
	}

	public string runFromScript(Plugin plugin) 
	{
		var start = new ProcessStartInfo()
		{
			FileName = plugin.FilePath,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		using (Process process = Process.Start(start))
		{
			using (StreamReader reader = process.StandardOutput)
			{
				return reader.ReadToEnd(); 
			}
		}
	}
}
