using HAL.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        public void RunFromScript(PluginFile plugin, IStorage storage)
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

                startProcess(storage, file, args);

                Consume();
            }));
        }

        private void startProcess(IStorage storage, string file, string args)
        {
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
        }
    }
}
