using HAL.CheckSum;
using HAL.Loggin;
using HAL.Plugin;
using server.serverFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{

    /// <summary>
    /// A user-defined object that contain information about receive operation
    /// </summary>
    internal class StateObject
    {
        public const int BufferSize = 2048;
        public byte[] buffer = new byte[BufferSize];

        public Socket server = null;
        public StringBuilder sb = new StringBuilder();

    }

    public class ServerFile
    {
        private const int Port = 11000;
        private const int nbMaxClient = 100;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartServer()
        {
            var pluginsFound = new List<PluginFileInfos>();

            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddr, Port);

            try
            {
                // Create a TCP Socket
                Socket listener = new Socket(ipAddr.AddressFamily,
                                             SocketType.Stream,
                                             ProtocolType.Tcp);

                listener.Bind(iPEndPoint);

                // Specify how many requests a Socket can listen before it gives Server busy response.
                listener.Listen(nbMaxClient);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCalback), listener);

                    if (allDone.WaitOne()) Console.WriteLine("Connexion set with a remote client !");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Log.Instance?.Error($"Error in StartServer() in ServerFile : {e.Message}\n{e.StackTrace}");
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCalback(IAsyncResult asyncResult)
        {
            // Signal to the .WaitOne() called in the main thread to continue
            allDone.Set();
            var listener = (Socket)asyncResult.AsyncState;
            var handler = listener.EndAccept(asyncResult);

            var stateObject = new StateObject();
            stateObject.server = handler;
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), stateObject);
        }

        public static void ReadCallback(IAsyncResult asyncResult)
        {
            var content = string.Empty;
            var pluginsFound = new List<PluginFileInfos>();

            var stateObject = (StateObject)asyncResult.AsyncState;
            var handler = stateObject.server;

            var bytesRead = handler.EndReceive(asyncResult);

            if (bytesRead > 0)
            {
                // store data receive so far
                stateObject.sb.Append(Encoding.UTF8.GetString(stateObject.buffer, 0, bytesRead));

                // Check for EOF
                content = stateObject.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // EOF have been detect so all the data has arrived
                    // Format the data to parse it and use it.
                    string[] strList = content.Split(";");
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

                    if(pluginToUpdate != null) SendFileAsync(handler, pluginToUpdate);
                } else
                {
                    handler.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), stateObject);
                }
            }
        }

        private static void SendFileAsync(Socket handler, List<PluginFileInfos> data)
        {
            foreach (var path in data)
            {
                // The preBuffer which is send, matching with the name of the plugins
                // TODO: Generify the path were to save plugin
                var preBuffer = Encoding.UTF8.GetBytes(String.Format("<FILE>{0}</FILE><PATH>plugins/</PATH>", path.FileName));
                // The postBuffer is the path of where to save it on the client machine
                var postBuffer = Encoding.UTF8.GetBytes(String.Format("<CHECKSUM>{0}</CHECKSUM>", path.CheckSum));
                Console.WriteLine(preBuffer.Length);
                handler.BeginSendFile(path.FilePath,
                                      preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread,
                                      new AsyncCallback(SendCallback), handler);
            }
        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var handler = (Socket)asyncResult.AsyncState;

                var bytesSent = handler.EndSend(asyncResult);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            } catch(Exception e)
            {
                Log.Instance?.Error($"SendCallBack in ServerFile : {e.Message}");
            }
        }

        /// <summary>
        /// Check the plugins that need to be updating and return a List of these or null.
        /// </summary>
        /// <param name="serverPlugins">The dictionary that contain all the plugins of the server-side plugins</param>
        /// <param name="clientPlugins">The List that contain all the plugins of the client-side plugins</param>
        /// <returns> A list of the path that need to be updating OR null if none</returns>
        private static List<PluginFileInfos> CheckAllPlugins(List<PluginFileInfos> serverPlugins, List<PluginFileInfos> clientPlugins)
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
        private static List<PluginFileInfos> PluginsOnServer()
        {
            var pluginsInfo = new List<PluginFileInfos>();

            foreach (var file in Directory.GetFiles("plugins"))
            {
                pluginsInfo.Add(new PluginSocketInfo(file, CheckSumGenerator.HashOf(file as string)));
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
                    // The preBuffer which is send, matching with the name of the plugins
                    // TODO: Generify the path were to save plugin
                    var preBuffer = Encoding.UTF8.GetBytes(String.Format("<FILE>{0}</FILE><PATH>plugins/</PATH>", path.FileName));
                    // The postBuffer is the path of where to save it on the client machine
                    var postBuffer = Encoding.UTF8.GetBytes(String.Format("<CHECKSUM>{0}</CHECKSUM>", path.CheckSum));
                    Console.WriteLine(preBuffer.Length);
                    handler.SendFile(path.FilePath, preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                Console.WriteLine(se.StackTrace);
                Log.Instance?.Error($"Socket error in SendFileToClient() ins ServerFile : {se.Message}\n{se.StackTrace}");
            }
        }


    }
}
