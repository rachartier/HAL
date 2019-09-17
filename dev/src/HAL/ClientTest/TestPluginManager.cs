using HAL.Plugin;
using HAL.Plugin.Mananger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ClientTest
{
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginManager_ValidScheldulePlugin()
        {
            PluginMaster pluginMaster = new PluginMaster();
            PluginManager pluginManager = new PluginManager(pluginMaster);

            BasePlugin plugin = new BasePlugin(pluginMaster, "test/script.py")
            {
                Activated = true,
                OsAuthorized = HAL.OSData.OSAttribute.TargetFlag.All
            };

            try
            {
                pluginManager.ScheldulePlugin(plugin);
            }
            catch (Exception e)
            {
                Assert.Fail($"ScheldulerService should have accepted this task: {e.Message}");
            }
        }

        public void PluginManager_ValidScheldulerTask()
        {
            const int nbPlugins = 10;

            PluginMaster pluginMaster = new PluginMaster();
            PluginManager pluginManager = new PluginManager(pluginMaster);

            for (int i = 0; i < nbPlugins; ++i)
            {
                pluginMaster.AddPlugin<BasePlugin>("test/script.py");
            }

            try
            {
                pluginManager.ScheldulePlugins(pluginMaster.Plugins);
            }
            catch (Exception e)
            {
                Assert.Fail($"ScheldulerService should have accepted this task: {e.Message}");
            }
        }
    }
}
