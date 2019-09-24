using Server;
using System;
using System.IO;

namespace server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("..//..//..");

            ServerFile server = new ServerFile();

            var d1 = DateTime.Now;
            var d2 = DateTime.Now;

            server.StartServer();
        }
    }
}
