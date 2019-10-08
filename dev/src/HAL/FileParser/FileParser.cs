using HAL.CheckSum;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HAL.FileParser
{
    public class FileParser
    {
        /// <summary>
        /// Parse a receive data which fit with the template of data send from server to client
        /// <FILE>{fileName}</FILE><PATH>{Path of the file in the client side}</PATH>{fileData}
        /// ...
        /// <CHECKSUM>{Checksum of the send file (to check)}</CHECKSUM><EOT>
        /// <EOT> is the End Of Transmission mark
        /// </summary>
        /// <param name="data">The data to parse</param>
        /// <returns></returns>
        public static string ParseAReceiveData(string data)
        {
            string path, fileName, checksum, content;

            var regFile = new Regex("(?<=<FILE>)(.*)(?=</FILE>)");
            var regPath = new Regex("(?<=<PATH>)(.*)(?=</PATH>)");
            var regFileContent = new Regex("(?<=</PATH>)(.*\\s*)(?=<CHECKSUM>)");
            var regChecksum = new Regex("(?<=<CHECKSUM>)(.*)(?=</CHECKSUM>)");

            fileName = MatchingRegex(regFile, data);
            path = MatchingRegex(regPath, data);
            checksum = MatchingRegex(regChecksum, data);

            var pathFileName = string.Concat(path, fileName);

            if (File.Exists(pathFileName))
            {
                File.Delete(pathFileName);
            }

            content = MatchingRegex(regFileContent, data);

            using (var fs = File.Create(pathFileName))
            {
                var text = Encoding.ASCII.GetBytes(content);
                fs.Write(text, 0, text.Length);
            }

            return CheckSumGenerator.HashOf(pathFileName);
        }

        private static string MatchingRegex(Regex regex, string data)
        {
            var match = regex.Match(data);
            if (match.Success)
            {
                return match.ToString();
            } else
            {
                return null;
            }
        }
    }
}
