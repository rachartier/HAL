using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.Plugin;
using System;
using System.Diagnostics;
using System.Threading;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
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

                try
                {
                    StartProcess(plugin, file, args);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    Consume();
                }
            }));
        }

        private void StartProcess(APlugin plugin, string file, string args)
        {
            if (OSData.OSAttribute.IsLinux)
            {
                ExecuteScriptLinux(plugin, file, args);
            }
            else if (OSData.OSAttribute.IsWindows)
            {
                ExecuteScriptWindows(plugin, file, args);
            }
        }

        private void ExecuteScriptWindows(APlugin plugin, string file, string args)
        {
            string verb = "";
            string username = "";

            // if the plugin is a powershell scripts, it needs some tweeks to execute it
            if (plugin.Infos.FileExtension.Equals(".ps1"))
            {
                file = "Powershell.exe";
                args = $"-executionpolicy remotesigned -File {args}";
            }

            var start = new ProcessStartInfo()
            {
                FileName = file,
                Arguments = args,
                Verb = verb,
                UserName = username,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(start);
            using var readerOutput = process.StandardOutput;
            using var readerInput = process.StandardError;

            string err = readerInput.ReadToEnd();

            if (!string.IsNullOrEmpty(err))
            {
                Log.Instance?.Error(err);
                return;
            }

            plugin.RaiseOnExecutionFinished(readerOutput.ReadToEnd());
        }

        private void ExecuteScriptLinux(APlugin plugin, string file, string args)
        {
            using var dllimport = new DllImportLaunchCmdUnix();
            string result = "";

            if (plugin.AdministratorRights)
            {
                result = dllimport.UseLaunchCommand($"sudo -u {plugin.AdministratorUsername} -s {file} -c {args}");
            }
            else
            {
                result = dllimport.UseLaunchCommand($"{file} {args}");
            }

            plugin.RaiseOnExecutionFinished(result);
        }
    }
}
