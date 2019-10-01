using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HAL.CheckSum
{
    public class CheckSumGenerator
    {
        /// <summary>
        /// Create a checksum in sha256 from a FileStream
        /// </summary>
        /// <param name="fileStream">The FileStream</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static string HashOf(FileStream fileStream)
        {
            StringBuilder sb = new StringBuilder();

            using (var mySha256 = SHA256.Create())
            {
                var data = mySha256.ComputeHash(fileStream);

                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Create a checksum in sha256 from a path file
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static string HashOf(string path)
        {
            StringBuilder sb = new StringBuilder();
            var checksum = new StringBuilder();

            var lines = File.ReadAllLines(path);
            using (var mySha256 = SHA256.Create())
            {
                foreach (var line in lines)
                {
                    sb.Append(line);
                }

                var hash = mySha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));

                for (int i = 0; i < hash.Length; i++)
                {
                    checksum.Append(hash[i].ToString("x2"));
                }
                return checksum.ToString();
            }
        }

        /// <summary>
        /// Create a checksum in sha256 from a simple data in string format
        /// </summary>
        /// <param name="data">The simple data in string</param>
        /// <returns>A sha256 checksum in string format</returns>
        public static string HashOfASimpleString(string data)
        {
            StringBuilder sb = new StringBuilder();

            using (var mySha256 = SHA256.Create())
            {
                var hash = mySha256.ComputeHash(Encoding.UTF8.GetBytes(data));

                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }
}
