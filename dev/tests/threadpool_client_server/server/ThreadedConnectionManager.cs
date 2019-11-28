using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace server
{
    internal class ThreadWithClients
    {
        public Thread Thread { get; set; }
        public List<TcpClientSavedState> Clients { get; } = new List<TcpClientSavedState>();
    }

    public class ThreadedConnectionManager
    {
        private ThreadWithClients[] threadPool;
        private int threadCount;
        private object keyAccessPool = new object();

        public ThreadedConnectionManager(int threadCount, int updateTimeMs = 1000)
        {
            this.threadCount = threadCount;

            threadPool = new ThreadWithClients[threadCount];

            for (int i = 0; i < threadCount; ++i)
            {
                threadPool[i] = new ThreadWithClients();
                var threadWitchClients = threadPool[i];

                threadWitchClients.Thread = new Thread(() =>
                {
                    while (true)
                    {
                            Parallel.For(0, threadWitchClients.Clients.Count, (index, _) =>
                            {
                                var client = threadWitchClients.Clients[index];
                                try
                                {
                                    client.Update();
                                }
                                catch
                                {
                                    client.Dispose();
                                    client.IsConnected = false;
                                }

                            });

                            threadWitchClients.Clients.RemoveAll((c => !c.IsConnected));

                        Thread.Sleep(updateTimeMs);
                    }
                });

                threadWitchClients.Thread.Start();
            }
        }

        public void KillAllConnections()
        {
            for (int i = 0; i < threadCount; ++i)
            {
                var threadWitchClients = threadPool[i];
                foreach (var client in threadWitchClients.Clients)
                {
                    client.Dispose();
                }

                threadWitchClients.Thread.Interrupt();
            }
        }

        public void AddTcpClient(TcpClientSavedState client)
        {
            var threadPool = this.threadPool[GetMinimumWorkingThread()];

            lock(keyAccessPool)
            {
                threadPool.Clients.Add(client);
            }
        }

        private int GetMinimumWorkingThread()
        {
            int threadIndex = 0;
            int minClientsInThread = threadPool[0].Clients.Count;

            for (int i = 1; i < threadCount; ++i)
            {
                var count = threadPool[i].Clients.Count;

                if (count < minClientsInThread)
                {
                    threadIndex = i;
                    minClientsInThread = count;
                }
            }

            return threadIndex;
        }

        public void _Info()
        {
            int totalClients = 0;

            Console.Clear();

            for (int i = 0; i < threadCount; ++i)
            {
                var thread = threadPool[i];

                Console.WriteLine($"Thread #{i}: {thread.Clients.Count} active clients");
                totalClients += thread.Clients.Count;
            }

            Console.WriteLine($"Total active clients: {totalClients}");
        }
    }
}