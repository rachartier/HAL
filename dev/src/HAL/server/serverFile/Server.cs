using HAL.Plugin;
using server.serverFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var pluginsFound = new List<PluginFileInfos>();

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
                    //Detect the EOF and break the loop
                    if (s.Equals("<EOF>"))
                    {
                        break;
                    }

                    pluginsFound.Add(Parser.ParseOnePluginFromData(s));
                }

                var pluginToUpdate = CheckAllPlugins(PluginsOnServer(), pluginsFound);

                if (pluginToUpdate != null)
                {
                    SendFileToClient(handler, pluginToUpdate);
                }

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
        /// <param name="serverFileName">Path of the server-side plugin</param>
        /// <param name="serverDate">Date of the server-side plugin</param>
        /// <param name="clientFileName">Path of the client-side plugin</param>
        /// <param name="clientDate">Date of the client-side plugin</param>
        /// <returns>
        /// 1: if the name of the server-side plugin is different than the client-side plugin OR if the date of the server-side plugin is later than the client-side plugin
        /// 0: if the name of the server-side plugin and the date are equals to the client-side one.
        /// -1: if the date of the server-side plugin is earlier than the client-side one.
        /// </returns>
        private int CheckPlugin(string serverFileName, DateTime serverDate, string clientFileName, DateTime clientDate)
        {
            if (ServerDateComparer.Compare(serverDate, clientDate) > 0 || !serverFileName.Equals(clientFileName))
            {
                return 1;
            }

            if (ServerDateComparer.Compare(serverDate, clientDate) == 0 || serverFileName.Equals(clientFileName))
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Check the plugins that need to be updating and return a List of these or null.
        /// </summary>
        /// <param name="serverPlugins">The dictionary that contain all the plugins of the server-side plugins</param>
        /// <param name="clientPlugins">The List that contain all the plugins of the client-side plugins</param>
        /// <returns> A list of the path that need to be updating OR null if none</returns>
        private List<PluginFileInfos> CheckAllPlugins(List<PluginFileInfos> serverPlugins, List<PluginFileInfos> clientPlugins)
        {
            var pluginToUpdate = new List<PluginFileInfos>();

            //TODO: Faire une vérif de taille
            //TODO: récupérer la différence de présence entre les deux dossiers plugins (LINQ?)

            if (clientPlugins.Count > serverPlugins.Count)
            {
                //Si le client possède un plugin de plus que le server, cela veux dire qu'il s'agit d'un plugin qui à été déposé
                //Directement sur le client, le serveur veut peut être essayé de le récupérer?
                Console.WriteLine("Client possède {0} alors que le Serveur en possède {1}", clientPlugins.Count, serverPlugins.Count);
            }

            var result = serverPlugins.Except(clientPlugins);

            pluginToUpdate = result.ToList();

            if (pluginToUpdate.Count == 0)
            {
                return null;
            }

            return pluginToUpdate;
        }

        /// <summary>
        /// Get all the plugin available on the server-side 
        /// </summary>
        /// <returns>A List of PluginFileInfos</returns>
        private List<PluginFileInfos> PluginsOnServer()
        {
            var pluginsInfo = new List<PluginFileInfos>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginsInfo.Add(new PluginFileInfos(file));
            }

            return pluginsInfo;
        }

        /// <summary>
        /// Send a list of file to the client
        /// </summary>
        /// <param name="handler">The handler of the actual connection </param>
        /// <param name="fileSendingList">List of file which need to be sent</param>
        private void SendFileToClient(Socket handler, List<PluginFileInfos> fileSendingList)
        {
            try
            {
                foreach (var path in fileSendingList)
                {
                    handler.SendFile(path.FilePath);
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
            return string.Format("plugins\\{0}", filePath.Split("\\", StringSplitOptions.RemoveEmptyEntries).GetValue(lenght - 1).ToString());
        }
    }
}
