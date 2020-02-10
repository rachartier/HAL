using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HAL.CheckSum
{
    public static class CheckSumGenerator
    {
        /// <summary>
        ///     Create a checksum in sha256 from a FileStream
        /// </summary>
        /// <param name="fileStream">The FileStream</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static string HashOf(FileStream fileStream)
        {
            var sb = new StringBuilder();

            using var mySha256 = SHA256.Create();
            var data = mySha256.ComputeHash(fileStream);

            for (var i = 0; i < data.Length; i++) sb.Append(data[i].ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        ///     Create a checksum in sha256 from a path file
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static async Task<string> HashOf(string path)
        {
            var sb = new StringBuilder();
            var lines = await File.ReadAllLinesAsync(path);
            foreach (var line in lines) sb.Append(line);

            return CheckSumComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        /// <summary>
        ///     Create a checksum in sha256 from a simple data in string format
        /// </summary>
        /// <param name="data">The simple data in string</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static string HashOfASimpleString(string data)
        {
            return CheckSumComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static string CheckSumComputeHash(byte[] data)
        {
            var sb = new StringBuilder();

            using var mySha256 = SHA256.Create();
            var hash = mySha256.ComputeHash(data);

            for (var i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }
    }
}