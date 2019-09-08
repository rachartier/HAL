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
        [DllImport("./lib/libreadso")]
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);
        private static readonly object key = new object();

        private string UseRunEntryPointSharedObject(string InputFile)
        {
            lock (key)
            {
                IntPtr result = run_entrypoint_sharedobject(Marshal.StringToHGlobalAnsi(InputFile));

                return Marshal.PtrToStringAnsi(result);
            }
        }

        public void RunFromSO(PluginFile plugin)
        {
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
