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
        public string MethodEntryPointName { get; set; } = "Run";
        public uint QueueLength { get; private set; } = 0u;

        private bool waitForComplete = false;
        private ManualResetEvent manualResetEvent;

        private static Dictionary<string, string> extensionConverterToIntepreterName = new Dictionary<string, string>();
        private static Dictionary<string, string> defaultExtensionName = new Dictionary<string, string>()
        {
            [".py"] = "python",
            [".rb"] = "ruby",
            [".pl"] = "perl",
            [".sh"] = ""
        };


        public PluginExecutor()
        {
            foreach (var fileType in PluginFile.AcceptedFilesTypes[PluginFile.FileType.Script])
            {
                string key = fileType;
                string val = "";

                var interpreterConfig = JSONConfigFile.Root["interpreter"];

                if (interpreterConfig == null)
                {
                    throw new NullReferenceException("interpter is not set in the configuration file.");
                }

                val = interpreterConfig[OSAttribute.GetOSFamillyName()].Value<string>(defaultExtensionName[fileType]);

                if (string.IsNullOrEmpty(val))
                {
                    val = defaultExtensionName[fileType];
                }

                extensionConverterToIntepreterName.Add(key, val);
            }
        }

        public void WaitForEmptyPool()
        {
            if (QueueLength == 0)
                return;

            manualResetEvent = new ManualResetEvent(false);
            waitForComplete = true;
            manualResetEvent.WaitOne();
        }

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