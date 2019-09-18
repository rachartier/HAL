using HAL.Plugin;
using HAL.Plugin.Executor;
using HAL.Plugin.Mananger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PluginTest
{
    [TestClass]
    public class UnitTest1
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

        [TestMethod]
        public void PluginExecutor_ValidWaitForEmptyPool()
        {
            const int nbInstances = 10;

            var pluginMaster = new PluginMaster();
            var executor = new PluginExecutor(pluginMaster);

            pluginMaster.AddPlugin<BasePlugin>("test/script.py");

            try
            {
                for (int i = 0; i < nbInstances; ++i)
                {
                    executor.RunFromScript(pluginMaster.Plugins[0]);
                }
            }
            catch(Exception) { }

            Assert.IsTrue(executor.WaitForEmptyPool(), "WaitForEmptyPool should have returned true");
        }

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

            BasePlugin plugin = new BasePlugin(pluginMaster, "test/script.py")
            {
                Activated = true,
                OsAuthorized = HAL.OSData.OSAttribute.TargetFlag.All
            };

            Assert.AreEqual(plugin.CanBeRun(), true, "Plugin should be executable");
        }
    }
}
