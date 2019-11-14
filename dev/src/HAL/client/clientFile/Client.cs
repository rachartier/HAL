using HAL.CheckSum;
using HAL.Loggin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HAL.Client
{
    /// <summary>
    /// A user-defined object that contain information about receive operation
    /// </summary>
    internal class StateObject
    {
        public const int BufferSize = 2048;
        public byte[] buffer = new byte[BufferSize];

        public Socket client = null;
        public StringBuilder sb = new StringBuilder();
    }

    internal class ClientFile
    {
        private const int Port = 11000;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private Dictionary<string, string> responseNameChecksum = new Dictionary<string, string>();

        public event EventHandler<EventArgs> OnReceiveDone;

        public void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);

                using var client = new Socket(ipAddr.AddressFamily,
                                           SocketType.Stream,
                                           ProtocolType.Tcp);

                try
                {
                    client.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallBack), client);
                    if (connectDone.WaitOne()) Log.Instance?.Debug("CONNECTION DONE");

                    //Get all the path of the plugins available on the current machine (plugins/)
                    string[] filePaths = Directory.GetFiles("plugins");
                    foreach (string path in filePaths)
                    {
                        //Encode the data to send to the server
                        byte[] data = Encoding.UTF8.GetBytes(string.Format(" {0} : {1};",
                            path, CheckSumGenerator.HashOf(path)));
                        Send(client, data);
                        sendDone.WaitOne();
                    }

                    byte[] eof = Encoding.UTF8.GetBytes("<EOF>");
                    // Send the data EOF through the socket;
                    Send(client, eof);
                    sendDone.WaitOne();

                    // Receive
                    Receive(client);
                    receiveDone.WaitOne();

                    // TODO: Compare the Equality of the Checksum (receive one and send one)
                    if (responseNameChecksum.Count > 0)
                    {
                        foreach (var content in responseNameChecksum)
                        {
                            if (CheckSumGenerator.HashOf(content.Key).Equals(content.Value))
                            {
                                Log.Instance?.Info($"{content.Key} have the same checksum that the received one.");
                            }
                            else
                            {
                                Log.Instance?.Info($"{content.Key} have not the same checksum that the received one.");
                            }
                        }
                    }

                    RaiseOnReceiveDone();

                    // Release the socket.
                    client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Log.Instance?.Error($"Client error: {e.Message}\n{e.StackTrace}");
                }
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Client error: {e.Message}\n{e.StackTrace}");
            }
        }

        private void ConnectCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;
                client.EndConnect(asyncResult);
                Log.Instance?.Info($"Socket connected to {client.RemoteEndPoint.ToString()}");

                // Signal that connection has been done
                connectDone.Set();
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Client ConnectCallBack error: {e.Message}\n{e.StackTrace}");
            }
        }

        private void ReceiveCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var stateObject = (StateObject)asyncResult.AsyncState;
                var client = stateObject.client;

                int bytesRead = client.EndReceive(asyncResult);

                var content = Encoding.UTF8.GetString(stateObject.buffer, 0, bytesRead);
                Log.Instance?.Debug($"Bytes Read: {bytesRead}");

                if (content.IndexOf("<EOF>") > -1)
                {
                    var checksum = FileParser.FileParser.ParseAReceiveData(content, out string pathFileName);
                    responseNameChecksum.Add(pathFileName, checksum);
                }

                if (bytesRead > 0)
                {
                    stateObject.sb.Append(content);

                    client.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), stateObject);
                }
                else
                {
                    // All data has arrived
                    Log.Instance?.Debug("All data has arrived");
                    //Signal that all data have been receive
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Client ReceiveCallback error: {e.Message}\n{e.StackTrace}");
            }
        }

        private void SendCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;
                int byteSend = client.EndReceive(asyncResult);

                Log.Instance?.Debug($"Sent {byteSend} bytes to server.");

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Log.Instance?.Error($"Client SendCallback error: {e.Message}\n{e.StackTrace}");
            }
        }

        private void Receive(Socket client)
        {
            var stateObject = new StateObject();
            stateObject.client = client;

            client.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), stateObject);
        }

        private void Send(Socket client, string data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), client);
        }

        private void Send(Socket client, byte[] data)
        {
            client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallBack), client);
        }

        private void RaiseOnReceiveDone()
        {
            OnReceiveDone?.Invoke(this, new EventArgs());
        }
    }
}