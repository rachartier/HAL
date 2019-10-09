using HAL.Plugin;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
    {
        /// <summary>
        /// default method entry point to execute the plugin's code
        /// </summary>
        public string MethodEntryPointName { get; set; } = "Run";

        public uint QueueLength { get; private set; } = 0u;

        public event EventHandler OnAllPluginsExecuted;

        private bool waitForComplete = false;
        private ManualResetEvent manualResetEvent;

        private readonly IPluginMaster refPluginMaster;

        public ThreadPoolPluginExecutor(IPluginMaster pluginMaster)
        {
            refPluginMaster = pluginMaster;
        }

        /*
         * libreadso allow to run multiple instance of classic dll or .so
         */

        [DllImport("./lib/libreadso")]
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);


        [DllImport("./lib/liblaunchcmdunix")]
        private static extern IntPtr launch_command(IntPtr command);

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
                try
                {
                    ptrString = Marshal.StringToHGlobalAnsi(InputFile);
                    IntPtr ptrResult = run_entrypoint_sharedobject(ptrString);

                    // result need to be converted to a string type, otherwise it can't be read and memory will be corrupted
                    return Marshal.PtrToStringAnsi(ptrResult);
                }
                catch (AccessViolationException)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// launch a command from the shell (unix) 
        /// </summary>
        /// <param name="command">command to be executedpath</param>
        /// <returns>the command's result</returns>
        private string UseLaunchCommand(string command)
        {
            lock (key)
            {
                IntPtr ptrString = Marshal.StringToHGlobalAnsi(command);
                IntPtr ptrResult = launch_command(ptrString);

                return Marshal.PtrToStringAnsi(ptrResult);
            }
        }

        /// <summary>
        /// wait until all workers have finished their jobs
        /// </summary>
        /// <returns>true if completed, false otherwise</returns>
        public bool WaitForEmptyPool()
        {
            if (QueueLength == 0)
            {
                return false;
            }

            manualResetEvent = new ManualResetEvent(false);
            waitForComplete = true;
            manualResetEvent.WaitOne();

            return true;
        }

        /// <summary>
        /// consume a worker and check if the worker was the last one
        /// </summary>
        private void Consume()
        {
            QueueLength--;

            if (QueueLength == 0)
            {
                OnAllPluginsExecuted?.Invoke(this, null);
                if (waitForComplete)
                {
                    waitForComplete = false;
                    manualResetEvent.Set();
                }
            }
        }
    }
}
