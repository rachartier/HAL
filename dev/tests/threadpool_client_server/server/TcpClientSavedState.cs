using System.Threading;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace server
{
    public abstract class TcpClientSavedState : IDisposable
    {
        public readonly StreamWriter StreamWriter; 
        public readonly StreamReader StreamReader;

        public bool IsConnected { get; set; }
        public bool IsFirstUpdate { get; set; } = true;

        private TcpClient reference;

        public TcpClientSavedState(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());

            reference = client;
            IsConnected = true;
        }

        public virtual async Task FirstUpdateAsync() 
        { 
            await Task.Run(() => {});
        }
        public abstract Task UpdateAsync();

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            reference.Dispose();
        }
    }
}