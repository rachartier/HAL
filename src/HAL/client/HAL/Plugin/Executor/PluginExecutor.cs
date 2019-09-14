using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        /// <summary>
        /// default method entry point to execute the plugin's code
        /// </summary>
        public string MethodEntryPointName { get; set; } = "Run";
        public uint QueueLength { get; private set; } = 0u;

        private bool waitForComplete = false;
        private ManualResetEvent manualResetEvent;

        private readonly PluginMaster refPluginMaster;

        public PluginExecutor(PluginMaster pluginMaster)
        {
            refPluginMaster = pluginMaster;
        }

        /*
         * libreadso allow to run multiple instance of classic dll or .so 
         */
        [DllImport("./lib/libreadso")]
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);
        private readonly object key = new object();

        /// <summary>
        /// run_entrypoint_sharedobject wrapper, to allocate the memory needed for the correct execution
        /// </summary>
        /// <param name="InputFile">dll/so path</param>
        /// <returns>the converted result string</returns>
        private string UseRunEntryPointSharedObject(string InputFile, out IntPtr ptrString)
        {
            lock (key)
            {
                ptrString = Marshal.StringToHGlobalAnsi(InputFile);
                IntPtr ptrResult = run_entrypoint_sharedobject(ptrString);

                // result need to be converted to a string type, otherwise it can't be read and memory will be corrupted
                return Marshal.PtrToStringAnsi(ptrResult);
            }
        }


        /// <summary>
        /// wait until all workers have finished their jobs
        /// </summary>
        public void WaitForEmptyPool()
        {
            if (QueueLength == 0)
            {
                return;
            }

            manualResetEvent = new ManualResetEvent(false);
            waitForComplete = true;
            manualResetEvent.WaitOne();
        }

        /// <summary>
        /// consume a worker and check if the worker was the last one
        /// </summary>
        private void Consume()
        {
            QueueLength--;

            if (waitForComplete && QueueLength == 0)
            {
                waitForComplete = false;
                manualResetEvent.Set();
            }
        }
    }
}
