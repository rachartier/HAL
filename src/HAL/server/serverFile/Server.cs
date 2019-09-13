using Server.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HALServer.Server
{
    public class ServerFile
    {
        private const int Port = 11000;
        private const int nbMaxClient = 10;

        public void StartServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddr, Port);

            //TODO: Penser à récupérer les DateTime de dernières écritures des plugins côté serveur
            var plugins = new List<PluginFile>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                plugins.Add(new PluginFile(file));
            }

            try
            {
                // Create a TCP Socket
                Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(iPEndPoint);

                // Specify how many requests a Socket can listen before it gives Server busy response.
                listener.Listen(nbMaxClient);

                Socket handler = listener.Accept();

                //prepare the data to come
                string data = null;
                //bytes is a buffer that contain the storage location for the received data
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                //Format the data to parse it and use it.
                String[] strList = data.Split(";");
                foreach (String s in strList)
                {
                    Console.WriteLine(s);
                    Parser.ParseOneDataPlugin(s);
                }

                //Optionnal echo message.
                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {

            }
        }

        public bool FileExist(String path)
        {
            return File.Exists(path)? true : false;
        }
    }
}
