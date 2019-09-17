using HAL.Plugin;
using HAL.Plugin.Executor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientTest
{
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginExecutor_ValidWaitForEmptyPool()
        {
            const int nbInstances = 1;

            var pluginMaster = new PluginMaster();
            var executor = new PluginExecutor(pluginMaster);

            pluginMaster.AddPlugin<BasePlugin>("test/script.py");

            for (int i = 0; i < nbInstances; ++i)
            {
                executor.RunFromScript(pluginMaster.Plugins[0]);
            }

            Assert.IsTrue(executor.WaitForEmptyPool(), "WaitForEmptyPool should have returned true");
        }

        [TestMethod]
        public void PluginMaster_InvalidWaitForEmptyPool()
        {
            var pluginMaster = new PluginMaster();
            var executor = new PluginExecutor(pluginMaster);

            Assert.IsFalse(executor.WaitForEmptyPool(), "WaitForEmptyPool should have returned false");
        }
    }
}