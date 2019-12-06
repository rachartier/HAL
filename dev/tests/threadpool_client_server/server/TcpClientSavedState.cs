using System;
using System.IO;
using System.Net.Sockets;

namespace server
{
    public abstract class TcpClientSavedState : IDisposable
    {
        public readonly StreamWriter StreamWriter; 
        public readonly StreamReader StreamReader;

        public bool IsConnected { get; set; }

        private TcpClient reference;

        public TcpClientSavedState(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());

            reference = client;
            IsConnected = true;
        }

        public abstract void Update();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            reference.Dispose();
        }
    }
}