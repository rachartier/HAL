using HAL.OSData;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        /// <summary>
        /// run a code from a shared object fille
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromSO(BasePlugin plugin)
        {
            // .so works only on linux
            if (OSAttribute.IsLinux)
            {
                QueueLength++;

                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    var result = UseRunEntryPointSharedObject(plugin.FilePath, out IntPtr ptrString);
                    plugin.RaiseOnExecutionFinished(result);

                    Marshal.FreeHGlobal(ptrString);

                    Consume();
                }));
            }
        }
    }
}
