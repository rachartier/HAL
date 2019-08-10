using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Threading;

public class PluginExecutor 
{
	public string MethodEntryPointName {get;set;} = "Run";
	public uint QueueLength = 0u;

	private bool waitForComplete = false;
	private ManualResetEvent manualResetEvent;

	public void RunFromDLL(Plugin plugin, IStorage storage)
	{
		QueueLength++;

		ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
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

			Consume();
		}));
	}

	public void RunFromScript(Plugin plugin, IStorage storage) 
	{
		QueueLength++;

		ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
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

			Consume();
		}));
	}

	public void WaitForEmptyPool()
	{
		if(QueueLength == 0)
			return;
		
		manualResetEvent = new ManualResetEvent(false);
		waitForComplete = true;
		manualResetEvent.WaitOne();
	}

	private void Consume()
	{
		QueueLength--;

		if(waitForComplete && QueueLength == 0)
		{
			waitForComplete = false;
			manualResetEvent.Set();
		}
	}
}
