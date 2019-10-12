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
        /// <returns>The checksum of the receive file</returns>
        public static string ParseAReceiveData(string data, out string pathFileName)
        {
            string path, fileName, checksum;

            var regFile = new Regex("(?<=<FILE>)(.*)(?=</FILE>)");
            var regFilePart = new Regex("(.*)(?=<PATH>)");

            var regPath = new Regex("(?<=<PATH>)(.*)(?=</PATH>)");
            var regPathPart = new Regex("(.*)(</PATH>)");

            var regChecksum = new Regex("(?<=<CHECKSUM>)(.*)(?=</CHECKSUM>)");
            var regChecksumPart = new Regex("(<CHECKSUM>)(.*)");

            fileName = MatchingRegex(regFile, regFilePart, ref data);
            path = MatchingRegex(regPath, regPathPart, ref data);
            checksum = MatchingRegex(regChecksum, regChecksumPart, ref data);

            pathFileName = string.Concat(path, fileName);

            if (File.Exists(pathFileName))
            {
                File.Delete(pathFileName);
            }

            using (var fs = File.Create(pathFileName))
            {
                var text = Encoding.UTF8.GetBytes(data);
                fs.Write(text, 0, text.Length);
            }

            return checksum;
        }

        private static string MatchingRegex(Regex regexFind, Regex regexDel, ref string data)
        {
            var match = regexFind.Match(data);
            if (match.Success)
            {
                if (regexDel != null)
                {
                    data = DeleteStringFromRegex(regexDel, data);
                }
                return match.ToString();
            }
            else
            {
                return null;
            }
        }

        private static string DeleteStringFromRegex(Regex regex, string data)
        {
            var matchDel = regex.Match(data);
            if (matchDel.Success)
            {
                data = data.Remove(matchDel.Index, matchDel.Length);
            }

            return data;
        }
    }
}