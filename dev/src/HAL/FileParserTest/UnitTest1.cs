using HAL.CheckSum;
using HAL.FileParser;
using HAL.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace FileParserTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void FileParser_ValidChecksumParsing()
        {
            var pluginSocketInfo = new PluginSocketInfo("plugins/date.pl", CheckSumGenerator.HashOf("plugins/date.pl"));

            var preBuffer = string.Format("<FILE>{0}</FILE><PATH>{1}</PATH>", pluginSocketInfo.FileName, "test/");
            var postBuffer = string.Format("<CHECKSUM>{0}</CHECKSUM>", pluginSocketInfo.CheckSum);

            var sb = new StringBuilder();

            var lines = File.ReadAllText(pluginSocketInfo.FilePath, Encoding.UTF8);

            foreach (var line in lines)
            {
                sb.Append(line);
            }

            var data = string.Concat(preBuffer, sb.ToString(), postBuffer);

            var checksum = FileParser.ParseAReceiveData(data, out string pathFileName);

            Assert.AreEqual(checksum, CheckSumGenerator.HashOf(pathFileName));
        }
    }
}
