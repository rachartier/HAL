using server.serverFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerFile
    {
        private const int Port = 11000;
        private const int nbMaxClient = 10;

        public void StartServer()
        {
            Dictionary<string, DateTime> pluginsFound = new Dictionary<string, DateTime>();

            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddr, Port);

            try
            {
                // Create a TCP Socket
                Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(iPEndPoint);

                // Specify how many requests a Socket can listen before it gives Server busy response.
                listener.Listen(nbMaxClient);


                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                //prepare the data to come
                string data = null;
                //bytes is a buffer that contain the storage location for the received data
                byte[] bytes = null;

                //Listen the data incoming until <EOF> is sending
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
                string[] strList = data.Split(";");
                foreach (string s in strList)
                {
                    Console.WriteLine(s);
                    //Detect the EOF and break the loop
                    if (s.Equals("<EOF>")) break;
                    (string path, DateTime date) = Parser.ParseOnePluginFromData(s);
                    pluginsFound.Add(path, date);
                }

                List<string> pluginToUpdate = CheckAllPlugins(PluginsOnServer(), pluginsFound);

                if (pluginToUpdate != null) SendFileToClient(handler, pluginToUpdate);

                //Optionnal echo message.
                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Check if the plugin information passed in param are different.
        /// </summary>
        /// <param name="serverPath">Path of the server-side plugin</param>
        /// <param name="serverDate">Date of the server-side plugin</param>
        /// <param name="clientPath">Path of the client-side plugin</param>
        /// <param name="clientDate">Date of the client-side plugin</param>
        /// <returns>
        /// 1: if the name of the server-side plugin is different than the client-side plugin OR if the date of the server-side plugin is later than the client-side plugin
        /// 0: if the name of the server-side plugin and the date are equals to the client-side one.
        /// -1: if the date of the server-side plugin is earlier than the client-side one.
        /// </returns>
        private int CheckPlugin(string serverPath, DateTime serverDate, string clientPath, DateTime clientDate)
        {
            var dateComparer = new ServerDateComparer();

            if (!serverPath.Equals(clientPath) || dateComparer.Compare(serverDate, clientDate) > 0) return 1;
            if (serverPath.Equals(clientPath) || dateComparer.Compare(serverDate, clientDate) == 0) return 0;

            return -1;
        }

        /// <summary>
        /// Check the plugins that need to be updating and return a List of these or null.
        /// </summary>
        /// <param name="serverPlugins">The dictionary that contain all the path (Key) and the last writen date (Value) of the server-side plugins</param>
        /// <param name="clientPlugins">The dictionary that contain all the path (Key) and the last writen date (Value) of the client-side plugins</param>
        /// <returns> A list of the path that need to be updating OR null if none</returns>
        private List<string> CheckAllPlugins(Dictionary<string, DateTime> serverPlugins, Dictionary<string, DateTime> clientPlugins)
        {
            List<string> pluginToUpdate = new List<string>();

            //TODO: ERREUR PARCOURS DES DICOS A CORRIGER
            foreach(KeyValuePair<string, DateTime> serverEntry in serverPlugins)
            {
                foreach(KeyValuePair<string, DateTime> clientEntry in clientPlugins)
                {
                    if (CheckPlugin(serverEntry.Key, serverEntry.Value, clientEntry.Key, clientEntry.Value) > 0)
                    {
                        pluginToUpdate.Add(clientEntry.Key);
                    } else
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("pluginToUpdate List count : {0} ", pluginToUpdate.Count);
            if (pluginToUpdate.Count == 0) return null;

            return pluginToUpdate;
        }

        /// <summary>
        /// Get all the plugin available on the server-side 
        /// </summary>
        /// <returns>A dictionary that contains a string for the path and a DateTime for the last written access of the plugins</returns>
        private Dictionary<string, DateTime> PluginsOnServer()
        {
            Dictionary<string, DateTime> pluginsOnServer = new Dictionary<string, DateTime>();

            var plugins = new List<String>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                plugins.Add(file);
            }

            var lenght = plugins.Count;

            foreach (var plugin in plugins)
            {
                pluginsOnServer.Add(plugin, File.GetLastWriteTime(plugin));
            }

            return pluginsOnServer;
        }

        /// <summary>
        /// Send a list of file to the client
        /// </summary>
        /// <param name="handler">The handler of the actual connection </param>
        /// <param name="fileSendingList">List of file which need to be sent</param>
        private void SendFileToClient(Socket handler, List<string> fileSendingList)
        {
            try
            {
                foreach(string path in fileSendingList)
                {
                    handler.SendFile(path);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                Console.WriteLine(se.StackTrace);
            }
        }

        private string GetPathFromRoot(string filePath)
        {
            int lenght = filePath.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length;
            return string.Format("plugins\\{0}",filePath.Split("\\", StringSplitOptions.RemoveEmptyEntries).GetValue(lenght-1).ToString());
        }

        private bool FileExist(String path)
        {
            return File.Exists(path)? true : false;
        }
    }
}
