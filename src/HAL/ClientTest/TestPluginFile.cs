using HAL.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientTest
{
    [TestClass]
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginFile_ValidExtensions()
        {
            PluginMaster pluginMaster = new PluginMaster();

            pluginMaster.AddPlugin<BasePlugin>("test/script.py");

            Assert.AreEqual(pluginMaster.Plugins[0].FileExtension, ".py", "Plugin extension not  recognized");
        }

        [TestMethod]
        public void PluginFile_ValidFileName()
        {
            PluginMaster pluginMaster = new PluginMaster();

            pluginMaster.AddPlugin<BasePlugin>("test/script.py");

            Assert.AreEqual(pluginMaster.Plugins[0].FileName, "script.py", "Bad plugin filename");
        }

        [TestMethod]
        public void PluginFile_ValidName()
        {
            PluginMaster pluginMaster = new PluginMaster();

            pluginMaster.AddPlugin<BasePlugin>("test/script.py");

            Assert.AreEqual(pluginMaster.Plugins[0].Name, "script", "Bad plugin name");
        }

        [TestMethod]
        public void PluginFile_ValidActivatedFalse()
        {
            PluginMaster pluginMaster = new PluginMaster();

            BasePlugin plugin = new BasePlugin(pluginMaster, "test/script.py");
            plugin.Activated = false;

            Assert.AreEqual(plugin.CanBeRun(), false, "Plugin shouldn't be executable");
        }

        [TestMethod]
        public void PluginFile_ValidActivatedTrue()
        {
            PluginMaster pluginMaster = new PluginMaster();

            BasePlugin plugin = new BasePlugin(pluginMaster, "test/script.py");
            plugin.Activated = true;
            plugin.OsAuthorized |= HAL.OSData.OSAttribute.TargetFlag.All;

            Assert.AreEqual(plugin.CanBeRun(), true, "Plugin should be executable");
        }
    }
}
