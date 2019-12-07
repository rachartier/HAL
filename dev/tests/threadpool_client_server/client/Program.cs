using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace client
{
    class Program
    {
        static int numberClients = 50;

        static Dictionary<int, bool> stats = new Dictionary<int, bool>();

        static object key = new object();

        static void Main(string[] args)
        {
            for (int i = 0; i < numberClients; ++i)
            {
                new Thread(() => {
                    using var client = new DemoClient(i, "127.0.0.1", 1664);

                    client.OnConnectionStateChanged += (o, e) =>
                    {
                        if(e.State == Client.ConnectionState.Updated) 
                        {
                            lock(key)
                            {
                                if (!stats.ContainsKey(client.Id))
                                {
                                    stats.Add(client.Id, true);
                                }
                            }
                        }
                    };

                    client.Start();
                }).Start();
            }

            while (true)
            {
                int clientsUpdated = 0;

                Console.Clear();
                Console.Write("Checking if all clients are udpated... ");

                lock(key) 
                {
                    foreach (var s in stats)
                    {
                        if (s.Value)
                            clientsUpdated++;
                    }
                }

                Console.WriteLine($"{clientsUpdated}/{numberClients}");
                Thread.Sleep(1);
            }
        }
    }
}