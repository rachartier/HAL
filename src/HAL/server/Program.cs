using Server;
using System;
using System.IO;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("..//..//..");

            ServerFile server = new ServerFile();

            server.StartServer();
        }
    }
}
