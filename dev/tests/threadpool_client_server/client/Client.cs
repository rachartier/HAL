using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace client
{
    public abstract class Client : IDisposable
    {
        public enum ConnectionState
        {
            None,
            Connected,
            Updated,
            Closed,
        }

        public class ConnectionUpdatedEventArgs
        {
            public readonly ConnectionState State;

            public ConnectionUpdatedEventArgs(ConnectionState state)
            {
                State = state;
            }
        }

        public event EventHandler<ConnectionUpdatedEventArgs> OnConnectionUpdated;
        public event EventHandler OnConnected;
        public event EventHandler OnClosed;

        public readonly StreamWriter StreamWriter;
        public readonly StreamReader StreamReader;

        public bool IsConnected {get;set;}

        private TcpClient client;

        private readonly int updateIntervalInMs;

        public Client(string ip, int port, int updateIntervalInMs = 100)
        {
            client = new TcpClient()
            {
                NoDelay = true,
            };

            this.updateIntervalInMs = updateIntervalInMs;


            client.Connect(ip, port);

            OnConnectionUpdated?.Invoke(this, new ConnectionUpdatedEventArgs(ConnectionState.Connected));
            OnConnected?.Invoke(this, null);

            IsConnected = true;

            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());
        }


        public void Start() 
        {
            HandleCommunication();

            OnConnectionUpdated?.Invoke(this, new ConnectionUpdatedEventArgs(ConnectionState.Closed));
            OnClosed?.Invoke(this, null);
        }

        public void HandleCommunication()
        {
            while (IsConnected)
            {
                Update();

                OnConnectionUpdated?.Invoke(this, new ConnectionUpdatedEventArgs(ConnectionState.Closed));
                Thread.Sleep(updateIntervalInMs);
            }
        }

        public abstract void Update();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            client.Dispose();
        }
    }
}