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
                        byte[] data = Encoding.ASCII.GetBytes(string.Format(" {0} : {1};",
                            path, CheckSumGenerator.HashOf(path)));
                        client.Send(data);
                    }

                    byte[] eof = Encoding.ASCII.GetBytes("<EOF>");
                    // Send the data EOF through the socket.    
                    int bytesSent = client.Send(eof);

                    // Receive the response from the remote device. (OPTIONNAL) 
                    int bytesRec = client.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

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
