using HAL.CheckSum;
using HAL.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using System.IO;

namespace ServerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Parser_ValidParsing()
        {
            var path = "test/script.py";

            var data = string.Format(" {0} : {1}", path, CheckSumGenerator.HashOf(path));
            PluginFileInfos pluginSent = new PluginSocketInfo(Path.GetFileName(path),
                                                 CheckSumGenerator.HashOf(path));
            PluginFileInfos pluginReceive = Parser.ParseOnePluginFromData(data);

            bool equal = pluginSent.Equals(pluginReceive);
            Assert.IsTrue(equal, "Plugin are supposed to be equals");
        }

        [TestMethod]
        public void Parser_InvalidParsing()
        {
            var pathRb = "test/script.rb";
            var pathPy = "test/script.py";

            string data = string.Format(" {0} : {1}", pathPy, CheckSumGenerator.HashOf(pathPy));
            PluginFileInfos pluginSent = new PluginSocketInfo(Path.GetFileName(pathRb),
                                                 CheckSumGenerator.HashOf(pathRb));
            PluginFileInfos pluginReceive = Parser.ParseOnePluginFromData(data);

            bool equal = pluginSent.Equals(pluginReceive);
            Assert.IsFalse(equal, "Plugin aren't supposed to be equals");
        }
    }
}
