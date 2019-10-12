using HAL.CheckSum;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HAL.Client
{
    internal class ClientFile
    {

        private const int Port = 11000;

        public void StartClient()
        {
            byte[] bytes = new byte[1024];


            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);

                Socket client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    client.Connect(ipEndPoint);

                    //Get all the path of the plugins available on the current machine (plugins/)
                    string[] filePaths = Directory.GetFiles("plugins");
                    foreach (string path in filePaths)
                    {
                        //Encode the data to send to the server
                        byte[] data = Encoding.UTF8.GetBytes(string.Format(" {0} : {1};",
                            path, CheckSumGenerator.HashOf(path)));
                        client.Send(data);
                    }

                    byte[] eof = Encoding.UTF8.GetBytes("<EOF>");
                    // Send the data EOF through the socket.    
                    int bytesSent = client.Send(eof);

                    // Receive
                    int bytesRec = client.Receive(bytes);
                    var checksumRec = FileParser.FileParser.ParseAReceiveData(Encoding.UTF8.GetString(bytes, 0, bytesRec),
                                                                              out string pathFileName);
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

    }
}
