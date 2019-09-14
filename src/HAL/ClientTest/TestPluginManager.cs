using HAL.Plugin;
using HAL.Plugin.Mananger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientTest
{
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginManager_ValidScheldulePlugin()
        {
            PluginMaster pluginMaster = new PluginMaster();
            PluginManager pluginManager = new PluginManager(pluginMaster);

            PluginFile plugin = new PluginFile(pluginMaster, "test/script.py")
            {
                Activated = true,
                OsAuthorized = HAL.OSData.OSAttribute.TargetFlag.All
            };

            bool result = pluginManager.ScheldulePlugin(plugin);

            Assert.AreEqual(result, true, "ScheldulerService should have accepted this task");
        }

        [TestMethod]
        public void PluginManager_InvalidScheldulePlugin()
        {
            PluginMaster pluginMaster = new PluginMaster();
            PluginManager pluginManager = new PluginManager(pluginMaster);

            PluginFile plugin = new PluginFile(pluginMaster, "test/script.py")
            {
                Activated = false
            };

            bool result = pluginManager.ScheldulePlugin(plugin);

            Assert.AreEqual(result, false, "ScheldulerService shouldn't have accepted this task");
        }

        public void PluginManager_ValidScheldulerTask()
        {
            const int nbPlugins = 10;

            PluginMaster pluginMaster = new PluginMaster();
            PluginManager pluginManager = new PluginManager(pluginMaster);

            for (int i = 0; i < nbPlugins; ++i)
            {
                pluginMaster.AddPlugin("test/script.py");
            }

            bool result = pluginManager.ScheldulePlugins(pluginMaster.Plugins);

            Assert.AreEqual(result, true, "ScheldulePlugins should avec returned true");
        }
    }
}
