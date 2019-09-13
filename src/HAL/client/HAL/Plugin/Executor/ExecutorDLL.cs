using HAL.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        /// <summary>
        /// run a code from a dll (dotnet dll and classical dll)
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromDLL(PluginFile plugin)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                try
                {
                    // will try to see if it is an assembly, then invoke the entry pointt
                    var assembly = Assembly.LoadFrom(plugin.FilePath);
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
                        throw new MethodAccessException($"Method '{MethodEntryPointName}' from DLL {plugin.FileName} not found.");
                    }
                }
                catch (Exception e)
                {
                    // otherwise, if it can't load assembly, it need to try a classic type file dll
                    if (e is System.BadImageFormatException || e is System.DllNotFoundException)
                    {
                        var result = UseRunEntryPointSharedObject(plugin.FilePath, out IntPtr ptrString);

                        plugin.RaiseOnExecutionFinished(result);

                        Marshal.FreeHGlobal(ptrString);
                    }
                }

                Consume();
            }));
        }
    }
}
