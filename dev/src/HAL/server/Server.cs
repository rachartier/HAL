using System;
using System.Net;
using System.Net.Sockets;

namespace HAL.Server
{
    internal class BaseServer
    {
        public delegate void DelegateClientConnected(object o, ClientStateChangedEventArgs tcpClient);

        public delegate void DelegateClientDisconnected(object o, ClientStateChangedEventArgs tcpClient);

        private readonly ThreadedConnectionManager connectionManager;
        private readonly TcpListener server;
        private bool isRunning = true;

        public DelegateClientConnected OnClientConnected;
        public DelegateClientConnected OnClientDisconnected;

        public BaseServer(string ip, int port, int allocatedThreads, int updateTimeMs)
        {
            connectionManager = new ThreadedConnectionManager(allocatedThreads, updateTimeMs);
            server = new TcpListener(IPAddress.Parse(ip), port);

            connectionManager.OnClientConnected += (o, e) => { OnClientConnected(o, e); };

            connectionManager.OnClientDisconnected += (o, e) => { OnClientDisconnected(o, e); };
        }

        public int? ClientsCount => connectionManager?.ClientsCount;
        public event EventHandler OnServerStarted;

        public event EventHandler OnServerClosed;

        public void Stop()
        {
            isRunning = false;
            connectionManager.KillAllConnections();
        }

        public void StartUniqueClientType<TClient>(string savePath)
            where TClient : TcpClientSavedState
        {
            server.Start();
            OnServerStarted?.Invoke(this, null);

            while (isRunning)
            {
                var client = server.AcceptTcpClient();
                var tcpOpenedStream = (TClient) Activator.CreateInstance(typeof(TClient), client, savePath);

                connectionManager.AddTcpClient(tcpOpenedStream);
            }

            OnServerClosed?.Invoke(this, null);
        }
    }
}