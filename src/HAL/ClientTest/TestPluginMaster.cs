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
        public void PluginMaster_ValidAddScriptExtension()
        {
            PluginMaster pluginMaster = new PluginMaster();

            try
            {
                pluginMaster.AddScriptExtension(".aa", "aaa");
            }
            catch(Exception e)
            {
                Assert.Fail("AddScriptExtension shouldn't had raised an exception");
            }
        }

        [TestMethod]
        public void PluginMaster_InvalidAddScriptExtension()
        {
            PluginMaster pluginMaster = new PluginMaster();

            pluginMaster.AddScriptExtension(".aa", "aaa");

            Assert.ThrowsException<ArgumentException>(() => pluginMaster.AddScriptExtension(".aa", ".bb"), "AddScriptExtension should had have raised an exception");
        }
    }
}