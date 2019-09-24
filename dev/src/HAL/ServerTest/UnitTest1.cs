using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using HAL.Plugin;
using System.IO;
using System;

namespace ServerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Parser_ValidParsing()
        {
            string data = "C:\\plugins\\test.c : 21/09/2019 12:18:00";
            PluginFileInfos pluginSent = new PluginSocketInfo(Path.GetFileName("C:\\plugins\\test.c"),
                                                 new DateTime(2019, 09, 21, 12, 18, 0));
            PluginFileInfos pluginReceive = Parser.ParseOnePluginFromData(data);

            bool equal = pluginSent.Equals(pluginReceive);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void Parser_InvalidParsing()
        {
            string data = " C:\\plugins\\test.c : 12/09/2019 12:18:00";
            PluginFileInfos pluginSent = new PluginSocketInfo(Path.GetFileName("C:\\plugins\\test.c"),
                                                 new DateTime(2019, 09, 21, 12, 18, 0));
            PluginFileInfos pluginReceive = Parser.ParseOnePluginFromData(data);

            bool equal = pluginSent.Equals(pluginReceive);
            Assert.IsFalse(equal);
        }
    }
}
