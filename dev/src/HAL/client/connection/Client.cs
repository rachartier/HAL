using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using HAL.Loggin;

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

        public StreamWriter StreamWriter {get; private set;}
        public StreamReader StreamReader {get; private set;}

        public bool IsConnected {get; protected set;}

        private TcpClient client;

        private readonly int updateIntervalInMs;

        private readonly string ip;
        private readonly int port;

        public BaseClient(string ip, int port, int updateIntervalInMs = 100)
        {
            this.ip = ip;
            this.port = port;
            this.updateIntervalInMs = updateIntervalInMs;

            Connect();
        }

        private void Connect()
        {   
            client = new TcpClient()
            {
                NoDelay = true,
            };
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
                if(!client.Connected)
                {
                    Log.Instance?.Info("No information from server... Will try to reconnect.");
                    IsConnected = false;
                    client.Close();

                    break;
                }

                await UpdateAsync();

                OnConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Updated));
                Thread.Sleep(updateIntervalInMs);
            }

            await TryReconnect();
        }

        public async Task TryReconnect() 
        {
            Log.Instance?.Info("Trying to reconnect...");
    
            while(!client.Connected)
            {
                try
                {
                    Connect();
                }
                catch(Exception e)
                {
                    Log.Instance?.Info($"Trying to reconnect... {e.Message}");
                    Thread.Sleep(1000);
                }
            }

            Log.Instance?.Info("Reconnection successful.");
            await StartAsync();
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