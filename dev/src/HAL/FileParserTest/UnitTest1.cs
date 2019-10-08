using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using HAL.Plugin;
using HAL.CheckSum;
using System.Text;
using HAL.FileParser;

namespace FileParserTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void FileParser_ValidChecksumParsing()
        {
            var pluginSocketInfo = new PluginSocketInfo("plugins/script.rb", CheckSumGenerator.HashOf("plugins/script.rb"));

            var preBuffer = string.Format("<FILE>{0}</FILE><PATH>{1}</PATH>", pluginSocketInfo.FileName, "test/");
            var postBuffer = string.Format("<CHECKSUM>{0}</CHECKSUM><EOT>", pluginSocketInfo.CheckSum);

            var sb = new StringBuilder();

            var lines = File.ReadAllLines(pluginSocketInfo.FilePath);
            foreach (var line in lines)
            {
                sb.Append(line);
            }

            var data = string.Concat(preBuffer, sb.ToString(), postBuffer);

            var returnCheckSum = FileParser.ParseAReceiveData(data);

            Assert.AreEqual(pluginSocketInfo.CheckSum, returnCheckSum);
        }
    }
}
