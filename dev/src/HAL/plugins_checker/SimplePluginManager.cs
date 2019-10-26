using HAL.Executor.ThreadPoolExecutor;
using HAL.Plugin;
using HAL.Plugin.Mananger;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugins_checker
{
    internal class SimplePluginManager : APluginManager
    {
        public SimplePluginManager(IPluginMaster pluginMaster)
        {
            Executor = new ThreadPoolPluginExecutor(pluginMaster);
        }
    }
}