using System;
using System.Diagnostics;
using System.Threading;
using HAL.Loggin;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        /// <summary>
        /// run a code from a script language
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromScript(APlugin plugin)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                string file = "";
                string args = plugin.Infos.FilePath;

                var fileExtension = plugin.Infos.FileExtension;

                // check if the extension is known
                if (!refPluginMaster.ExtensionsNames.ContainsKey(plugin.Infos.FileExtension))
                {
                    throw new ArgumentException("Unknown extension.");
                }

                file = refPluginMaster.ExtensionsNames[fileExtension];

                // check if the extension had been set
                if (string.IsNullOrEmpty(file))
                {
                    throw new ArgumentNullException($"Value {refPluginMaster.ExtensionsNames[fileExtension]} from interpreter object in json file not found.");
                }

                startProcess(plugin, file, args);

                Consume();
            }));
        }

        private void startProcess(APlugin plugin, string file, string args)
        {
            string verb = "";

            if (plugin.AdministratorRights)
            {
                if (OSData.OSAttribute.IsLinux)
                {
                    args = $"-u {plugin.AdministratorUsername} --shell {file} {args}";
                    file = $"sudo";

                    Console.WriteLine($"{file} {args}");
                }
                /*              
                               else if (OSData.OSAttribute.IsWindows)
                               {
                                   verb = "runas";
                               }
               */
            }

            var start = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = args,
                Verb = verb,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(start))
            {
                using (var reader = process.StandardOutput)
                {
                    plugin.RaiseOnExecutionFinished(reader.ReadToEnd());
                }
                using (var reader = process.StandardError)
                {
                    string err = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(err))
                    {
                        Log.Instance?.Error(err);
                    }
                }
            }
        }
    }
}
