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
        static readonly int numberClients = 10;

        static void Main(string[] args)
        {
            List<Thread> children = new List<Thread>();

            for (int i = 0; i < numberClients; ++i)
            {
                var thread = new Thread(async () => {
                    int concI = i;

                    using var client = new DemoClient(concI, "127.0.0.1", 1664);

                    await client.StartAsync();
                });

                thread.Start();
                children.Add(thread);
            }

            foreach(var thread in children) 
            {
                thread.Join();
            }

            Console.WriteLine("All clients are disconnected.");
        }
    }
}