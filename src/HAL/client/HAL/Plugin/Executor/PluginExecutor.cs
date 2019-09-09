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

        private static IDictionary<string, string> extensionConverterToIntepreterName = new Dictionary<string, string>();


        /// <summary>
        /// default extension name, you need to add yourself an entry to add a script language 
        /// </summary>
        private static IDictionary<string, string> defaultExtensionName = new Dictionary<string, string>()
        {
            [".py"] = "python",
            [".rb"] = "ruby",
            [".pl"] = "perl",
            [".sh"] = "bash",
            [".lua"] = "lua",
        };

        public PluginExecutor()
        {
            foreach (var fileType in PluginFile.AcceptedFilesTypes[PluginFile.FileType.Script])
            {
                string key = fileType;
                string val = "";

                // an interpreter is needed to interpret the code
                var interpreterConfig = JSONConfigFile.Root["interpreter"];

                if (interpreterConfig == null)
                {
                    throw new NullReferenceException("interpter is not set in the configuration file.");
                }

                // an intepreter can change depending the os
                val = interpreterConfig[OSAttribute.GetOSFamillyName()].Value<string>(defaultExtensionName[fileType]);

                // if it can't be found, the default one is choose
                if (string.IsNullOrEmpty(val))
                {
                    val = defaultExtensionName[fileType];
                }

                extensionConverterToIntepreterName.Add(key, val);
            }
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
