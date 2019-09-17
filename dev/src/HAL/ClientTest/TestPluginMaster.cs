using HAL.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ClientTest
{
    public partial class TestPlugin
    {
        [TestMethod]
        public void PluginMaster_ValidAddScriptExtension()
        {
            PluginMaster pluginMaster = new PluginMaster();

            try
            {
                pluginMaster.AddScriptExtension(".aa", "aaa");
            }
            catch (Exception e)
            {
                Assert.Fail($"AddScriptExtension shouldn't had raised an exception: {e.Message}");
            }
        }

        [TestMethod]
        public void PluginMaster_InvalidAddScriptExtension()
        {
            PluginMaster pluginMaster = new PluginMaster();

            pluginMaster.AddScriptExtension(".aa", "aaa");

            Assert.ThrowsException<ArgumentException>(() => pluginMaster.AddScriptExtension(".aa", "aaa"), "AddScriptExtension should had have raised an exception");
        }
    }
}