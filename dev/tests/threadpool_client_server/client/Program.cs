using System.Buffers.Text;
using System.Net.Sockets;
using System;
using System.IO;
using System.Threading;

namespace client
{
    class Client
    {
        private TcpClient client;
        private bool isConnected;

        public Client(int id, string ip, int port)
        {
            client = new TcpClient()
            {
                NoDelay = true,
            };

            client.Connect(ip, port);

            Console.WriteLine($"Hello World ({id})");

            HandleCommunication(id);
        }

        public void HandleCommunication(int id)
        {
            using var streamWriter = new StreamWriter(client.GetStream());

            using var streamReader = new StreamReader(client.GetStream());

            bool isConnected = true;

            streamWriter.Write($"{id}\n");
            streamWriter.Flush();

            while (isConnected)
            {
                try
                {
                    string dataReceived = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(dataReceived))
                    {
                        isConnected = false;
                    }

                    //Console.WriteLine($"received {id}: {dataReceived}");
                }
                catch
                {
                    isConnected = false;
                }
            }

            Console.WriteLine($"Goodbye World ({id})...");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 20; ++i)
            {
                new Thread(() =>
                {
                    Client client = new Client(i, "127.0.0.1", 1664);
                }).Start();
            }
            while (true) { }
        }
    }
}
