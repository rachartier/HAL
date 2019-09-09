using HAL.OSData;
using HAL.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        /*
         * libreadso allow to run multiple instance of classic dll or so 
         */
        [DllImport("./lib/libreadso")]
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);
        private static readonly object key = new object();

        /// <summary>
        /// run_entrypoint_sharedobject wrapper, to allocate the memory needed for the correct execution
        /// </summary>
        /// <param name="InputFile">dll/so path</param>
        /// <returns></returns>
        private string UseRunEntryPointSharedObject(string InputFile)
        {
            lock (key)
            {
                IntPtr result = run_entrypoint_sharedobject(Marshal.StringToHGlobalAnsi(InputFile));

                // result need to be converted to a string type, otherwise it can't be read
                return Marshal.PtrToStringAnsi(result);
            }
        }

        /// <summary>
        /// run a code from a shared object fille
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromSO(PluginFile plugin)
        {
            // .so works only on linux
            if (OSAttribute.IsLinux)
            {
                QueueLength++;

                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    var result = UseRunEntryPointSharedObject(plugin.FilePath);
                    plugin.RaiseOnExecutionFinished(result);

                    Consume();
                }));
            }
        }
    }
}
