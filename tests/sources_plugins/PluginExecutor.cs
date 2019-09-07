using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

public class PluginExecutor
{
    public string MethodEntryPointName { get; set; } = "Run";
    public uint QueueLength { get; private set; } = 0u;

    private bool waitForComplete = false;
    private ManualResetEvent manualResetEvent;

    private static Dictionary<string, string> extensionConverterToIntepreterName = new Dictionary<string, string>();
    private static Dictionary<string, string> defaultExtensionName = new Dictionary<string, string>()
    {
        [".py"] = "python",
        [".rb"] = "ruby",
        [".pl"] = "perl",
        [".sh"] = ""
    };

    [DllImport("./lib/libreadso")]
    private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);

    private string UseRunEntryPointSharedObject(string InputFile)
    {
        IntPtr result = run_entrypoint_sharedobject(Marshal.StringToHGlobalAnsi(InputFile));
        return Marshal.PtrToStringAnsi(result);
    }

    public PluginExecutor()
    {
        foreach (var fileType in Plugin.AcceptedFilesTypes[Plugin.FileType.Script])
        {
            string key = fileType;
            string val = "";

            var interpreterConfig = JSONConfigFile.Root["interpreter"];

            if (interpreterConfig == null)
            {
                throw new NullReferenceException("interpter is not set in the configuration file.");
            }

            val = interpreterConfig[OSAttribute.GetOSFamillyName()].Value<string>(defaultExtensionName[fileType]);

            if (string.IsNullOrEmpty(val))
            {
                val = defaultExtensionName[fileType];
            }

            extensionConverterToIntepreterName.Add(key, val);
        }
    }

    public void RunFromSO(Plugin plugin, IStorage storage)
    {
        if(OSAttribute.IsLinux)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                var result = UseRunEntryPointSharedObject(plugin.FilePath);

                storage.Save(result);

                Consume();
            }));
        }
    }

    public void RunFromDLL(Plugin plugin, IStorage storage)
    {
        QueueLength++;

        ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
        {
            try
            {
                var assembly = Assembly.LoadFrom(plugin.FilePath);
                var type = assembly.GetTypes().FirstOrDefault();

                var entryPointMethod = type?.GetMethod(MethodEntryPointName);

                if (entryPointMethod != null)
                {
                    dynamic instance = Activator.CreateInstance(type);
                    dynamic result = entryPointMethod.Invoke(instance, null);

                    storage.Save(result);
                }
                else
                {
                    throw new MethodAccessException($"Method 'Run' from DLL {plugin.FileName} not found.");
                }
            }
            catch (Exception e)
            {
                if(e is System.BadImageFormatException || e is System.DllNotFoundException)
                {
                    var result = UseRunEntryPointSharedObject(plugin.FilePath);

                    storage.Save(result);
                }
            }

            Consume();
        }));
    }

    public void RunFromScript(Plugin plugin, IStorage storage)
    {
        QueueLength++;

        ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
        {
            string file = "";
            string args = plugin.FilePath;

            var fileExtension = plugin.FileExtension;

            if (!extensionConverterToIntepreterName.ContainsKey(plugin.FileExtension))
            {
                throw new ArgumentException("Unknown extension.");
            }

            file = extensionConverterToIntepreterName[fileExtension];

            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException($"Value {defaultExtensionName[fileExtension]} from interpreter object in json file not found.");
            }

            var start = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(start))
            {
                using (var reader = process.StandardOutput)
                {
                    storage.Save(reader.ReadToEnd());
                }
            }

            Consume();
        }));
    }

    public void WaitForEmptyPool()
    {
        if (QueueLength == 0)
            return;

        manualResetEvent = new ManualResetEvent(false);
        waitForComplete = true;
        manualResetEvent.WaitOne();
    }

    private void Consume()
    {
        QueueLength--;

        if (waitForComplete && QueueLength == 0)
        {
            waitForComplete = false;
            manualResetEvent.Set();
        }
    }
}
