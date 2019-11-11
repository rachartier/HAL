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

        private static int counterFile;
        private static int dataSize;

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);

        public ServerFile()
        {
        }

        public static void StartServer()
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

                    Log.Instance?.Info($"Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCalback), listener);

                    if (allDone.WaitOne()) Log.Instance?.Info($"Connexion set with a remote client !");
                }

            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Error in StartServer() in ServerFile : {e.Message}\n{e.StackTrace}");
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        /// <summary>
        /// The Callback that allow the server to receive, it's the acknowledgement of a connection. Allow to begin a reception from the remote connection
        /// </summary>
        /// <param name="asyncResult">The asynchronous result where the states object is store</param>
        public static void AcceptCalback(IAsyncResult asyncResult)
        {
            var listener = (Socket)asyncResult.AsyncState;
            var handler = listener.EndAccept(asyncResult);

            // Signal to the .WaitOne() called in the main thread to continue
            allDone.Set();

            var stateObject = new StateObject();
            stateObject.server = handler;
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), stateObject);
        }

        /// <summary>
        /// The Callback for the reading part. It's call when the receive have been done.
        /// This method read the receives data, format them and then send it to parsing for using it as HAL.Plugin.PluginFileInfos
        /// </summary>
        /// <param name="asyncResult">The asynchronous result where the states object is store</param>
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

                    if (pluginToUpdate != null)
                    {
                        SendFileAsync(handler, pluginToUpdate);
                    }
                    else
                    {
                        Log.Instance?.Info("Close Connection");
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }


                } else
                {
                    handler.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), stateObject);
                }
            }
        }

        /// <summary>
        /// Send file asynchronously to the handler, the files are passed in args into a List of HAL.Plugin.PluginFileInfos.
        /// Format files for sending into correct format.
        /// <FILE></FILE><PATH></PATH>content of file<CHECKSUM></CHECKSUM><EOF>
        /// </summary>
        /// <param name="handler">The handler where to send file</param>
        /// <param name="data">A list of HAL.Plugin.PluginFileInfos to send to the handler</param>
        private static void SendFileAsync(Socket handler, List<PluginFileInfos> data)
        {
            dataSize = data.Count;

            for (counterFile = 1; counterFile <= data.Count; counterFile++)
            {
                // The preBuffer which is send, matching with the name of the plugins
                // TODO: Generify the path were to save plugin
                var preBuffer = String.Format("<{1}><FILE>{0}</FILE><PATH>plugins/</PATH>", data[counterFile-1].FileName, counterFile);
                // The postBuffer is the path of where to save it on the client machine
                var postBuffer = String.Format("<CHECKSUM>{0}</CHECKSUM></{1}><EOF>", data[counterFile-1].CheckSum, counterFile);

                SendDataFile(handler, data[counterFile-1].FilePath, preBuffer, postBuffer, data.Count);
            }
        }

        private static void SendData(Socket handler, String data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);
            Log.Instance?.Debug($"SendData data string: {data}");

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// Enconding the data and then begin a send to the handler
        /// </summary>
        /// <param name="handler">The endpoint where to send the datas</param>
        /// <param name="pathName">The path name of the sending file</param>
        /// <param name="preBuffer">the prebuffer of the send</param>
        /// <param name="postBuffer">the postbuffer of the send</param>
        /// <param name="dataSize">the full size of the data to find, usefull for sync the close connection</param>
        private static void SendDataFile(Socket handler, string pathName, string preBuffer, string postBuffer, int dataSize)
        {
            var preBuffBytes = Encoding.UTF8.GetBytes(preBuffer);
            var postBufferBytes = Encoding.UTF8.GetBytes(postBuffer);

            Log.Instance?.Debug($"SendDataFile file send: {pathName}");
            Log.Instance?.Debug($"SendDataFile full data send: {preBuffer}{pathName}{postBuffer}");

            handler.BeginSendFile(pathName, preBuffBytes, postBufferBytes, TransmitFileOptions.UseDefaultWorkerThread, new AsyncCallback(SendFileCallback), handler);
            if (counterFile == dataSize)
            {
                Log.Instance?.Debug("Wake up the signal to close socket");
                sendDone.Set();
            }
        }

        /// <summary>
        /// The Callback for the end of sending files. When all the files have been sent, close the remote connection.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result where the states object is store</param>
        private static void SendFileCallback(IAsyncResult asyncResult)
        {
            try
            {
                var handler = (Socket)asyncResult.AsyncState;

                handler.EndSendFile(asyncResult);
                Log.Instance?.Info("File Correctly sent");
                Log.Instance?.Debug($"datasize {dataSize} counterFile {counterFile}");
                if (counterFile >= dataSize)
                {
                    Log.Instance?.Debug("Wait for the signal to come up");
                    sendDone.WaitOne();
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    Log.Instance?.Debug("Connection Close");
                }
            } catch(Exception e)
            {
                Log.Instance?.Error($"SendFileCallBack in ServerFile : {e.Message}");
            }
        }

        /// <summary>
        /// The Callback for the end of sending data. When done, close the remote connection.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result where the states object is store</param>
        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var handler = (Socket)asyncResult.AsyncState;

                var bytesSent = handler.EndSend(asyncResult);
                Log.Instance?.Debug($"SendCallBack dataSent : {bytesSent}");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Log.Instance?.Debug("Connection Close");
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
