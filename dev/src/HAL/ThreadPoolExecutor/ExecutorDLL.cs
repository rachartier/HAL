using HAL.Executor;
using HAL.Plugin;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
    {
        /// <summary>
        /// run a code from a dll (dotnet dll and classical dll)
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromDLL(APlugin plugin)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                try
                {
                    tryRunAssemblyDLL(plugin);
                }
                catch (Exception e)
                {
                    // if it can't load assembly, it need to try a classic type file dll
                    if (e is System.BadImageFormatException || e is System.DllNotFoundException)
                    {
                        runClassicDLL(plugin);
                    }
                }

                Consume();
            }));
        }

        private void tryRunAssemblyDLL(APlugin plugin)
        {
            var assembly = Assembly.LoadFrom(plugin.Infos.FilePath);
            var type = assembly.GetTypes().FirstOrDefault();

            var entryPointMethod = type?.GetMethod(MethodEntryPointName);

            if (entryPointMethod != null)
            {
                dynamic instance = Activator.CreateInstance(type);
                dynamic result = entryPointMethod.Invoke(instance, null);

                plugin.RaiseOnExecutionFinished(result);
            }
            else
            {
                throw new MethodAccessException($"Method '{MethodEntryPointName}' from DLL {plugin.Infos.FileName} not found.");
            }
        }

        private void runClassicDLL(APlugin plugin)
        {
            var result = UseRunEntryPointSharedObject(plugin.Infos.FilePath, out IntPtr ptrString);

            plugin.RaiseOnExecutionFinished(result);

            Marshal.FreeHGlobal(ptrString);
        }
    }
}
