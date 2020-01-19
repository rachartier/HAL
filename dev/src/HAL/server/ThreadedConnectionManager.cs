using HAL.Loggin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAL.Server
{
    internal class ThreadWithClients
    {
        public Thread UpdateThread { get; set; }
        public List<TcpClientSavedState> Clients { get; } = new List<TcpClientSavedState>();
    }

    public class ThreadedConnectionManager
    {
        private readonly ThreadWithClients[] threadPool;
        private readonly int threadCount;
        private readonly object keyAccessPool = new object();

        public event EventHandler<ClientStateChangedEventArgs> OnClientConnected;
        public event EventHandler<ClientStateChangedEventArgs> OnClientDisconnected;

        public ThreadedConnectionManager(int threadCount, int updateTimeMs = 1000, int heartbeatWaitTimeMs = 5_000)
        {
            this.threadCount = threadCount;

            threadPool = new ThreadWithClients[threadCount];

            for (int i = 0; i < threadCount; ++i)
            {
                threadPool[i] = new ThreadWithClients();
                var threadWitchClients = threadPool[i];

                threadWitchClients.UpdateThread = new Thread(() =>
                {
                    while (true)
                    {
                        Parallel.ForEach(threadWitchClients.Clients, async (client) =>
                        {
                            try
                            {
                                client.SendHeartbeat();
                            }
                            catch
                            {
                                client.Disconnect();
                                client.Dispose();
                                OnClientDisconnected?.Invoke(this, new ClientStateChangedEventArgs(client));
                                return;
                            }

                            if (client.Stopwatch.ElapsedMilliseconds > updateTimeMs || client.IsFirstUpdate)
                            {
                                try
                                {
                                    if (client.IsFirstUpdate)
                                    {
                                        OnClientConnected?.Invoke(this, new ClientStateChangedEventArgs(client));

                                        try
                                        {
                                            await client.FirstUpdateAsync();
                                        }
                                        catch
                                        {
                                            // Not very beautiful, but we don't really care if the streams are already occupied.
                                            // If one fail, then, wait the next cycle...
                                        }

                                        client.IsFirstUpdate = false;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await client.UpdateAsync();
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Instance?.Error($"ThreadedConnectionManager: {e.Message}");

                                    client.Disconnect();
                                    client.Dispose();
                                    OnClientDisconnected?.Invoke(this, new ClientStateChangedEventArgs(client));
                                }

                                client.Stopwatch.Restart();
                            }
                            else
                            {
                                try
                                {
                                    await client.SaveAsync();
                                }
                                catch
                                {
                                }
                            }
                        });

                        threadWitchClients.Clients.RemoveAll(c => !c.IsConnected);
                        threadWitchClients.Clients.RemoveAll(c => !c.IsConnected);
                        threadWitchClients.Clients.RemoveAll(c => !c.IsConnected);
                        Thread.Sleep(10);
                    }
                });

                threadWitchClients.UpdateThread.Start();
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

                threadWitchClients.UpdateThread.Interrupt();
            }
        }

        public void AddTcpClient(TcpClientSavedState client)
        {
            lock (keyAccessPool)
            {
                var threadSlot = threadPool[GetMinimumWorkingThread()];
                threadSlot.Clients.Add(client);
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
    }
}
