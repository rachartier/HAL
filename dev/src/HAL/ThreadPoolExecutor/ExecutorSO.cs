using HAL.OSData;
using HAL.Plugin;
using HAL.Loggin;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security;
using System.Runtime.ExceptionServices;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor

    {
        /// <summary>
        /// run a code from a shared object fille
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromSO(APlugin plugin)
        {
            // .so works only on linux
            if (OSAttribute.IsLinux)
            {
                QueueLength++;

                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    try
                    {
                        var result = UseRunEntryPointSharedObject(plugin.Infos.FilePath, out IntPtr ptrString);
                        plugin.RaiseOnExecutionFinished(result);

                        Marshal.FreeHGlobal(ptrString);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        Consume();
                    }
                }));
            }
        }
    }
}
