using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace server
{
    public abstract class TcpClientOpenedStream : IDisposable
    {
        public StreamWriter StreamWriter { get; }
        public StreamReader StreamReader { get; }

        public bool IsDisconnected { get; set; } = false;

        public TcpClientOpenedStream(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());
        }

        public abstract void Update();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();
        }
    }

    internal class ThreadWitchClients
    {
        public Thread Thread { get; set; }
        public List<TcpClientOpenedStream> Clients { get; } = new List<TcpClientOpenedStream>();
        public object key = new object();
    }

    public class ThreadedConnectionManager
    {
        private ThreadWitchClients[] threadsPool;

        private int threadIndex = 0;
        private int threadCount;

        public ThreadedConnectionManager(int threadCount)
        {
            this.threadCount = threadCount;

            threadsPool = new ThreadWitchClients[threadCount];

            for (int i = 0; i < threadCount; ++i)
            {
                threadsPool[i] = new ThreadWitchClients();
                var threadOpenedStream = threadsPool[i];

                threadOpenedStream.Thread = new Thread(() =>
                {
                    while (true)
                    {
                        Parallel.For(0, threadOpenedStream.Clients.Count, (index, _) =>
                        {
                            var client = threadOpenedStream.Clients[index];

                            try
                            {
                                client.Update();
                            }
                            catch
                            {
                                client.Dispose();
                                client.IsDisconnected = true;
                            }
                        });


                        threadOpenedStream.Clients.RemoveAll((c => c.IsDisconnected));

                        Thread.Sleep(100);
                    }
                });

                threadOpenedStream.Thread.Start();
            }
        }

        public void AddTcpClient(TcpClientOpenedStream client)
        {
            var threadPool = threadsPool[threadIndex];

            lock (threadPool.key)
            {
                threadPool.Clients.Add(client);
            }

            threadIndex = ((threadIndex + 1) % threadCount);
        }

        public void _Info()
        {
            int totalClients = 0;

            Console.Clear();

            for (int i = 0; i < threadCount; ++i)
            {
                var thread = threadsPool[i];

                Console.WriteLine($"Thread #{i}: {thread.Clients.Count} active clients");
                totalClients += thread.Clients.Count;
            }

            Console.WriteLine($"Total active clients: {totalClients}");
        }
    }
}