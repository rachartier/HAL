﻿using HAL.OSData;
using HAL.Plugin;
using System;
using System.Runtime.InteropServices;
using System.Threading;

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
                    var result = UseRunEntryPointSharedObject(plugin.Infos.FilePath, out IntPtr ptrString);
                    plugin.RaiseOnExecutionFinished(result);

                    Marshal.FreeHGlobal(ptrString);

                    Consume();
                }));
            }
        }
    }
}