using System.Buffers.Text;
using System.Net.Sockets;
using System;
using System.IO;
using System.Threading;

namespace client
{
    class Client
    {
        public static int NumberClients = 50;
        public static bool Kill = false;
        public static bool[] Updated = new bool[NumberClients + 1];

        private TcpClient client;
        private bool isConnected;

        public Client(int id, string ip, int port)
        {
            client = new TcpClient()
            {
                NoDelay = true,
            };

            client.Connect(ip, port);
            //Console.WriteLine($"Hello World ({id})");

            Updated[id] = false;

            HandleCommunication(id);
        }

        public void HandleCommunication(int id)
        {
            using var streamWriter = new StreamWriter(client.GetStream());
            using var streamReader = new StreamReader(client.GetStream());

            bool isConnected = true;

            streamWriter.Write($"{id}\n");
            streamWriter.Flush();

            while (isConnected && !Kill)
            {
                try
                {
                    string dataReceived = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(dataReceived))
                    {
                        isConnected = false;
                    }
                    //Console.WriteLine($"received {id}: {dataReceived}");
                    Updated[id] = true;
                }
                catch
                {
                    isConnected = false;
                }
            }

            client.Dispose();
            Console.WriteLine($"Goodbye World ({id})...");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < Client.NumberClients; ++i)
            {
                new Thread(() =>
                {
                    Client client = new Client(i, "127.0.0.1", 1664);
                }).Start();
            }

            new Thread(() =>
            {
                while (true)
                {
                    bool good = true;
                    int updated = 0;

                    Console.Clear();
                    Console.Write("Checking if all clients are udpated... ");

                    for (int i = 0; i < Client.NumberClients; ++i)
                    {
                        if (!Client.Updated[i])
                            good = false;
                        else
                            updated++;
                    }

                    if (good)
                    {
                        Console.WriteLine("GOOD");
                    }
                    else
                    {
                        Console.WriteLine($"NOT GOOD ({updated} updated)");
                    }

                    Thread.Sleep(1000);
                }
            }).Start();

            Console.WriteLine("Press any key to kill all clients...");
            Console.ReadKey();

            Client.Kill = true;
        }
    }
}
