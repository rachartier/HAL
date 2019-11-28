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
        static int numberClients = 100;

        static Dictionary<int, bool> stats = new Dictionary<int, bool>();

        static object key = new object();

        static int clientId = 0;

        static void Main(string[] args)
        {

            for (int i = 0; i < numberClients; ++i)
            {
                new Thread(() =>
                {
                    using var client = new DemoClient(i, "127.0.0.1", 1664);

                    client.OnConnectionUpdated += (o, e) =>
                    {
                        lock(key)
                        {
                            if (!stats.ContainsKey(client.Id))
                            {
                                stats.Add(client.Id, true);
                            }
                        }
                    };

                    client.Start();
                }).Start();
            }

            new Thread(() =>
            {
                while (true)
                {
                    int clientsUpdated = 0;

                    Console.Clear();
                    Console.Write("Checking if all clients are udpated... ");

                    foreach (var s in stats)
                    {
                        if (s.Value)
                            clientsUpdated++;
                    }

                    Console.WriteLine($"{clientsUpdated}/{numberClients}");

                    Thread.Sleep(1000);
                }
            }).Start();

            Console.WriteLine("Press any key to kill all clients...");
            Console.ReadKey();
        }
    }
}