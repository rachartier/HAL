﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.Plugin;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
    {
        /// <summary>
        ///     run a code from a dll (dotnet dll and classical dll)
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromDLL(APlugin plugin)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(obj =>
            {
                // TODO: verifier si il y a un moyen de connaitre si c est une dll dotnet

                try
                {
                    TryRunAssemblyDLL(plugin);
                }
                catch (Exception e)
                {
                    // if it can't load assembly, it need to try a classic type file dll
                    if (e is BadImageFormatException) RunClassicDLL(plugin);
                }
                finally
                {
                    Consume();
                }
            });
        }

        private void TryRunAssemblyDLL(APlugin plugin)
        {
            var assembly = Assembly.LoadFrom(plugin.Infos.FilePath);
            var type = assembly.GetTypes().FirstOrDefault();

            var entryPointMethod = type?.GetMethod(MethodEntryPointName);

            if (entryPointMethod != null)
            {
                dynamic instance = Activator.CreateInstance(type);
                var result = entryPointMethod.Invoke(instance, null);

                plugin.RaiseOnExecutionFinished(result);
            }
            else
            {
                Log.Instance?.Error($"Method '{MethodEntryPointName}' from DLL {plugin.Infos.FileName} not found.");
            }
        }

        private void RunClassicDLL(APlugin plugin)
        {
            using var dllimport = new DllImportEntryPoint();

            var result = dllimport.UseRunEntryPointSharedObject(plugin.Infos.FilePath);

            plugin.RaiseOnExecutionFinished(result);
        }
    }
}