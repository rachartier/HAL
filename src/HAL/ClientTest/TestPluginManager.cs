using HAL.Plugin;
using HAL.Plugin.Mananger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientTest
{
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginManager_ValidScheldulerTask()
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
        public void PluginManager_InvalidScheldulerTask()
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
    }
}
