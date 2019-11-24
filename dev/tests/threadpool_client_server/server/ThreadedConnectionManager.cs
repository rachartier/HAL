using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace server
{
    public abstract class TcpClientSavedState : IDisposable
    {
        public StreamWriter StreamWriter { get; }
        public StreamReader StreamReader { get; }

        public bool IsDisconnected { get; set; } = false;

        private TcpClient reference;

        public TcpClientSavedState(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());

            reference = client;
        }

        public abstract void Update();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            reference.Dispose();
        }
    }

    internal class ThreadWithClients
    {
        public Thread Thread { get; set; }
        public List<TcpClientSavedState> Clients { get; } = new List<TcpClientSavedState>();
    }

    public class ThreadedConnectionManager
    {
        private ThreadWithClients[] threadsPool;
        private int threadCount;

        public ThreadedConnectionManager(int threadCount, int updateTimeMs)
        {
            this.threadCount = threadCount;

            threadsPool = new ThreadWithClients[threadCount];

            for (int i = 0; i < threadCount; ++i)
            {
                threadsPool[i] = new ThreadWithClients();
                var threadWitchClients = threadsPool[i];

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
                                client.IsDisconnected = true;
                            }
                        });

                        threadWitchClients.Clients.RemoveAll((c => c.IsDisconnected));

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
                var threadWitchClients = threadsPool[i];
                foreach (var client in threadWitchClients.Clients)
                {
                    client.Dispose();
                }

                threadWitchClients.Thread.Interrupt();
            }
        }

        public void AddTcpClient(TcpClientSavedState client)
        {
            var threadPool = threadsPool[GetMinimumWorkingThread()];

            threadPool.Clients.Add(client);
        }

        private int GetMinimumWorkingThread()
        {
            int threadIndex = 0;
            int minClientsInThread = threadsPool[0].Clients.Count;

            for (int i = 1; i < threadCount; ++i)
            {
                var count = threadsPool[i].Clients.Count;

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
                var thread = threadsPool[i];

                Console.WriteLine($"Thread #{i}: {thread.Clients.Count} active clients");
                totalClients += thread.Clients.Count;
            }

            Console.WriteLine($"Total active clients: {totalClients}");
        }
    }
}