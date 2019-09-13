using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using HAL.Storage;
using HAL.OSData;
using HAL.Storage.Configuration;

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

        private PluginMaster refPluginMaster;

        public PluginExecutor(PluginMaster pluginMaster)
        {
            refPluginMaster = pluginMaster;
        }

        /// <summary>
        /// wait until all workers have finished their jobs
        /// </summary>
        public void WaitForEmptyPool()
        {
            if (QueueLength == 0)
                return;

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
