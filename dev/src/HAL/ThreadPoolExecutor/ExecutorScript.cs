﻿using HAL.Loggin;
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

                startProcess(plugin, file, args);

                Consume();
            }));
        }

        private string base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void startProcess(APlugin plugin, string file, string args)
        {
            string verb = "";
            string username = "";

            if (plugin.AdministratorRights)
            {
                if (OSData.OSAttribute.IsLinux)
                {
                    string result = UseLaunchCommand($"sudo -u {plugin.AdministratorUsername} -s {file} -c {args}");
                    plugin.RaiseOnExecutionFinished(result);

                    return;
                }
                else if (OSData.OSAttribute.IsWindows)
                {
                    username = plugin.AdministratorUsername;
                    verb = "runas";
                }
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