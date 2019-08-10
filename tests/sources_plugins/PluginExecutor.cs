using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Threading;

public class PluginExecutor 
{
	public string MethodEntryPointName {get;set;} = "Run";

	public void runFromDLL(Plugin plugin, IStorage storage)
	{
		new Thread(() => {
			var assembly = Assembly.LoadFrom(plugin.FilePath);
			var type = assembly.GetTypes().FirstOrDefault();

			if(type == null)
			{
				throw new NullReferenceException("Bad assembly type.");
			}

			MethodInfo entryPointMethod = type.GetMethod(MethodEntryPointName);

			dynamic instance = Activator.CreateInstance(type);
			dynamic result = entryPointMethod.Invoke(instance, null);

			storage.Save(result);
		}).Start();
	}

	public void runFromScript(Plugin plugin, IStorage storage) 
	{
		new Thread(() => {
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
					storage.Save(reader.ReadToEnd());
				}
			}
	  }).Start();
	}
}
