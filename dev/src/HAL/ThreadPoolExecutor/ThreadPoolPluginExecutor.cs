using HAL.Plugin;
using System;
using System.Threading;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor
    {
        private readonly IPluginMaster refPluginMaster;
        private ManualResetEvent manualResetEvent;

        public event EventHandler OnAllPluginsExecuted;

        /// <summary>
        /// default method entry point to execute the plugin's code
        /// </summary>
        public string MethodEntryPointName { get; set; } = "Run";

        public uint QueueLength { get; private set; } = 0u;

        private bool waitForComplete = false;

        public ThreadPoolPluginExecutor(IPluginMaster pluginMaster)
        {
            refPluginMaster = pluginMaster;
        }

        /*
		 * libreadso allow to run multiple instance of classic dll or .so
		 */

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