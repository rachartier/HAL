using System;
using System.Net.Sockets;


namespace server 
{
    class Server
    {
        public event EventHandler OnServerStarted;
        public event EventHandler OnServerClosed;

        public delegate void DelegateClientConnected(object o, ClientStateChangedEventArgs tcpClient);
        public delegate void DelegateClientDisconnected(object o, ClientStateChangedEventArgs tcpClient);

        public DelegateClientConnected OnClientConnected;
        public DelegateClientConnected OnClientDisconnected;

        private ThreadedConnectionManager connectionManager;
        private TcpListener server;
        private bool isRunning = true;

        public Server(string ip, int port, int allocatedThreads)
        {
            connectionManager = new ThreadedConnectionManager(allocatedThreads);
            server = new TcpListener(System.Net.IPAddress.Parse(ip), port);

            connectionManager.OnClientConnected += (o,e) => {
                OnClientConnected (o, e);
            };

            connectionManager.OnClientDisconnected += (o,e) => {
                OnClientDisconnected (o, e);
            };
        }

        public async void StartUniqueClientType<TClient>() 
            where TClient : TcpClientSavedState
        {
            server.Start();
            OnServerStarted?.Invoke(this, null);

            while (isRunning)
            {
                var client = await server.AcceptTcpClientAsync();
                var tcpOpenedStream = (TClient)Activator.CreateInstance(typeof(TClient), client);

                connectionManager.AddTcpClient(tcpOpenedStream);
            }

            OnServerClosed?.Invoke(this, null);
        }
    }
}