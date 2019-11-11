using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    Log.Instance?.Error("Unknown extension.");
                }

                file = refPluginMaster.ExtensionsNames[fileExtension];

                // check if the extension had been set
                if (string.IsNullOrEmpty(file))
                {
                    Log.Instance?.Error($"Value {refPluginMaster.ExtensionsNames[fileExtension]} from interpreter object in json file not found.");
                    return;
                }

                try
                {
                    StartProcess(plugin, file, args);
                }
                catch (Exception e)
                {
                    Log.Instance?.Error($"{plugin.Infos.FileName} encountered a problem: {e.Message}");
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
                throw new ApplicationException(err);
            }

            plugin.RaiseOnExecutionFinished(readerOutput.ReadToEnd());
        }

        private void ExecuteScriptLinux(APlugin plugin, string file, string args)
        {
            using var dllimport = new DllImportLaunchCmdUnix();
            string result = "";

            if (plugin.AdministratorRights)
            {
                result = dllimport.UseLaunchCommand($"sudo -u {plugin.AdministratorUsername} -s {file} -c {args} 2> /dev/null");
            }
            else
            {
                result = dllimport.UseLaunchCommand($"{file} {args} 2> /dev/null");
            }

            // if it's not a valid json
            try
            {
                JObject.Parse(result);
            }
            catch (JsonReaderException)
            {
                Log.Instance?.Error(result);
                return;
            }

            plugin.RaiseOnExecutionFinished(result);
        }
    }
}