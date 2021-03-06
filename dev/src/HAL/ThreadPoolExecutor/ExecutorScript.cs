﻿using System;
using System.Diagnostics;
using System.Threading;
using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.OSData;
using HAL.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
    {
        /// <summary>
        ///     run a code from a script language
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromScript(APlugin plugin)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(obj =>
            {
                var file = "";
                var args = plugin.Infos.FilePath;

                var fileExtension = plugin.Infos.FileExtension;

                // check if the extension is known
                if (!refPluginMaster.ExtensionsNames.ContainsKey(plugin.Infos.FileExtension))
                    Log.Instance?.Error("Unknown extension.");

                file = refPluginMaster.ExtensionsNames[fileExtension];

                // check if the extension had been set
                if (string.IsNullOrEmpty(file))
                {
                    Log.Instance?.Error(
                        $"Value {refPluginMaster.ExtensionsNames[fileExtension]} from interpreter object in json file not found.");
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
            });
        }

        private void StartProcess(APlugin plugin, string file, string args)
        {
            if (OSAttribute.IsLinux)
                ExecuteScriptLinux(plugin, file, args);
            else if (OSAttribute.IsWindows) ExecuteScriptWindows(plugin, file, args);
        }

        private void ExecuteScriptWindows(APlugin plugin, string file, string args)
        {
            var verb = "";
            var username = "";

            // if the plugin is a powershell scripts, it needs some tweeks to execute it
            if (plugin.Infos.FileExtension.Equals(".ps1"))
            {
                file = "Powershell.exe";
                args = $"-executionpolicy remotesigned -File {args}";
            }

            var start = new ProcessStartInfo
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

            var err = readerInput.ReadToEnd();

            if (!string.IsNullOrEmpty(err))
            {
                Log.Instance?.Error($"Plugin encoutered an error: {err}");
                throw new ApplicationException(err);
            }

            plugin.RaiseOnExecutionFinished(readerOutput.ReadToEnd());
        }

        private void ExecuteScriptLinux(APlugin plugin, string file, string args)
        {
            using var dllimport = new DllImportLaunchCmdUnix();
            var result = "";

            if (plugin.AdministratorRights)
                result = dllimport.UseLaunchCommand(
                    $"sudo -u {plugin.AdministratorUsername} -s {file} -c {args} 2> /dev/null");
            else
                result = dllimport.UseLaunchCommand($"{file} {args} 2> /dev/null");

            // if it's not a valid json
            try
            {
                JObject.Parse(result);
            }
            catch (JsonReaderException)
            {
                Log.Instance?.Error($"{plugin.Infos.Name}: result not in a json format: {result}");
                return;
            }

            plugin.RaiseOnExecutionFinished(result);
        }
    }
}