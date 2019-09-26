using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HAL.CheckSum
{
    public class CheckSumGenerator
    {

        public static string HashOf(FileStream fileStream)
        {
            StringBuilder sb = new StringBuilder();

            using (var mySha256 = SHA256.Create())
            {
                var data = mySha256.ComputeHash(fileStream);

                for (int i=0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                return sb.ToString();
            }

        }

        public static string HashOf(string data)
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
