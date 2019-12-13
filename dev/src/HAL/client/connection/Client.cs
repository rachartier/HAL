using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HAL.Connection.Client
{
    public abstract class BaseClient : IDisposable
    {
        public enum ConnectionState
        {
            None,
            Connected,
            Updated,
            Closed,
        }

        public class ConnectionStateChangedEventArgs
        {
            public readonly ConnectionState State;

            public ConnectionStateChangedEventArgs(ConnectionState state)
            {
                State = state;
            }
        }

        public event EventHandler<ConnectionStateChangedEventArgs> OnConnectionStateChanged;
        public event EventHandler OnConnected;
        public event EventHandler OnClosed;

        public readonly StreamWriter StreamWriter;
        public readonly StreamReader StreamReader;

        public bool IsConnected {get; protected set;}

        private TcpClient client;

        private readonly int updateIntervalInMs;

        public BaseClient(string ip, int port, int updateIntervalInMs = 100)
        {
            client = new TcpClient()
            {
                NoDelay = true,
            };

            this.updateIntervalInMs = updateIntervalInMs;
            client.Connect(ip, port);

            IsConnected = true;

            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());
        }

        public async Task StartAsync() 
        {
            OnConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connected));
            OnConnected?.Invoke(this, new EventArgs());

            await HandleCommunicationAsync();

            OnConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Closed));
            OnClosed?.Invoke(this, new EventArgs());
        }

        public async Task HandleCommunicationAsync()
        {
            while (IsConnected)
            {
                await UpdateAsync();

                OnConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Updated));
                Thread.Sleep(updateIntervalInMs);
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public abstract Task UpdateAsync();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            client.Dispose();
        }
    }
}