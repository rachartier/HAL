using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAL.Loggin;

namespace HAL.Server
{
    internal class ThreadWithClients
    {
        public Thread UpdateThread { get; set; }
        public List<TcpClientSavedState> Clients { get; } = new List<TcpClientSavedState>();
    }

    public class ThreadedConnectionManager
    {
        private readonly object keyAccessPool = new object();
        private readonly int threadCount;

        private readonly ThreadWithClients[] threadPool;

        public ThreadedConnectionManager(int threadCount, int updateTimeMs = 1000, int heartbeatWaitTimeMs = 5_000)
        {
            this.threadCount = threadCount;

            threadPool = new ThreadWithClients[threadCount];

            for (var i = 0; i < threadCount; ++i)
            {
                threadPool[i] = new ThreadWithClients();
                var threadWitchClients = threadPool[i];

                threadWitchClients.UpdateThread = new Thread(() =>
                {
                    while (true)
                    {
                        Parallel.ForEach(threadWitchClients.Clients, async client =>
                        {
                            if (client.HeartbeatStopwatch.ElapsedMilliseconds > client.HeartbeatUpdateTimeMs)
                            {
                                client.HeartbeatStopwatch.Restart();

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
                            }

                            if (client.Stopwatch.ElapsedMilliseconds > updateTimeMs || client.IsFirstUpdate)
                            {
                                client.Stopwatch.Restart();

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
                                            // ignored
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
                            }
                            else
                            {
                                try
                                {
                                    await client.SaveAsync();
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        });

                        threadWitchClients.Clients.RemoveAll(c => !c.IsConnected);
                        Thread.Sleep(10);
                    }
                });

                threadWitchClients.UpdateThread.Start();
            }
        }

        public int ClientsCount
        {
            get
            {
                var total = 0;
                
                lock (keyAccessPool)
                {
                    foreach (var t in threadPool) total += t.Clients.Count;
                }
                
                return total;
            }
        }

        public event EventHandler<ClientStateChangedEventArgs> OnClientConnected;

        public event EventHandler<ClientStateChangedEventArgs> OnClientDisconnected;

        public void KillAllConnections()
        {
            for (var i = 0; i < threadCount; ++i)
            {
                var threadWitchClients = threadPool[i];
                foreach (var client in threadWitchClients.Clients) client.Dispose();

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
            var threadIndex = 0;
            var minClientsInThread = threadPool[0].Clients.Count;

            for (var i = 1; i < threadCount; ++i)
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