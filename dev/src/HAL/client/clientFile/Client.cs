using HAL.CheckSum;
using System;
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

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static string response = String.Empty;

        public void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);

                Socket client = new Socket(ipAddr.AddressFamily,
                                           SocketType.Stream,
                                           ProtocolType.Tcp);

                try
                {
                    client.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallBack), client);
                    if(connectDone.WaitOne()) Console.WriteLine("CONNECTION DONE");

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
                    int bytesRec = client.Receive(bytes);
                    Receive(client);
                    if (receiveDone.WaitOne())
                    {
                        var checksum = FileParser.FileParser.ParseAReceiveData(response, out string pathFileName);
                    }

                    // TODO: Compare the Equality of the Checksum (receive one and send one)


                    // Release the socket.    
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private static void ConnectCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;
                client.EndConnect(asyncResult);
                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that connection has been done
                connectDone.Set();
            } catch (Exception e)
            {
                // Socket error...
            }
        }

        private static void ReceiveCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var stateObject = (StateObject)asyncResult.AsyncState;
                var client = stateObject.client;

                int bytesRead = client.EndReceive(asyncResult);

                if(bytesRead <= 0)
                {
                    stateObject.sb.Append(Encoding.UTF8.GetString(stateObject.buffer, 0, bytesRead));

                    client.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), stateObject);
                } else
                {
                    // All data has arrived
                    if (stateObject.sb.Length >= 1)
                    {
                        response = stateObject.sb.ToString();
                    }
                    //Signal that all data have been receive
                    receiveDone.Set();
                }

            } catch(Exception e)
            {
                // Socket error...
            }
        }

        private static void SendCallBack(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;
                int byteSend = client.EndReceive(asyncResult);

                Console.WriteLine("Sent {0} bytes to server.", byteSend);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            } catch
            {
                //Socket error...
            }
        }

        private static void Receive(Socket client)
        {
            var stateObject = new StateObject();
            stateObject.client = client;

            client.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), stateObject);
        }

        private static void Send(Socket client, string data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), client);
        }

        private static void Send(Socket client, byte[] data)
        {
            client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallBack), client);
        }

    }
}
