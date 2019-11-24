using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace server
{
    class Server
    {
        private TcpListener server;
        private bool isRunning;
        private object key = new object();
        public Dictionary<int, int> ClientStat = new Dictionary<int, int>();

        public Server(string ip, int port)
        {

            server = new TcpListener(System.Net.IPAddress.Parse(ip), port);

            server.Start();

            Console.WriteLine("Server is running. Waiting for clients...");

            isRunning = true;


            new Thread(() =>
            {
                while (true)
                {
                    Console.Clear();

                    lock (key)
                    {
                        foreach (var stat in ClientStat)
                        {
                            Console.WriteLine($"#{stat.Key}: {stat.Value}");
                        }

                        Console.WriteLine($"Total: {ClientStat.Keys.Count}");
                    }

                    Thread.Sleep(2 * 1000);
                }
            }).Start();

            var mainThread = new Thread(async () =>
            {
                while (isRunning)
                {
                    var client = await server.AcceptTcpClientAsync();

                    ThreadPool.QueueUserWorkItem(async (_) =>
                    {
                        await HandleClient(client);
                    });
                }
            });

            mainThread.Start();
            mainThread.Join();
        }

        private async Task HandleClient(TcpClient client)
        {
            var random = new Random();

            using var streamWriter = new StreamWriter(client.GetStream());
            using var streamReader = new StreamReader(client.GetStream());

            bool isClientConnected = true;
            string dataToSend = "volvic is the best water";
            int id = 0;

            id = int.Parse(streamReader.ReadLine());
            //Console.WriteLine($"Client #{id} has logged in...");

            while (isClientConnected)
            {
                //Console.WriteLine($"Sending: \"{dataToSend}\" to client #{id}");

                try
                {
                    await streamWriter.WriteLineAsync(dataToSend);
                    await streamWriter.FlushAsync();

                    lock (key)
                    {
                        if (!ClientStat.ContainsKey(id))
                        {
                            ClientStat.Add(id, 0);
                        }

                        ClientStat[id] += 1;
                    }
                }
                catch
                {
                    isClientConnected = false;
                }

                Thread.Sleep((1 + random.Next(5)) * 1000);
            }

            //Console.WriteLine($"Client #{id} disconnected.");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server("127.0.0.1", 1664);
        }
    }
}