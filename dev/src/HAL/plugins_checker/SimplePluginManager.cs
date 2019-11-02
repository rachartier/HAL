using HAL.Executor.ThreadPoolExecutor;
using HAL.Plugin;
using HAL.Plugin.Mananger;

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