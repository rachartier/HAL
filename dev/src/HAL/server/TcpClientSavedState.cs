using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using HAL.MagicString;

namespace HAL.Server
{
    public abstract class TcpClientSavedState : IDisposable
    {
        public readonly StreamWriter StreamWriter;
        public readonly StreamReader StreamReader;

        public readonly Stopwatch Stopwatch;

        public bool IsConnected { get; set; }
        public bool IsFirstUpdate { get; set; } = true;

        public readonly TcpClient reference;

        public TcpClientSavedState(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());

            Stopwatch = new Stopwatch();

            Stopwatch.Start();

            reference = client;
            IsConnected = true;
        }

        public virtual async Task FirstUpdateAsync()
        {
            await Task.Run(() => { });
        }

        public abstract Task UpdateAsync();

        public abstract Task SaveAsync();

        public void SendHeartbeat()
        {
            StreamWriter.WriteLine(MagicStringEnumerator.CMDHtb);
        }

        public void Disconnect()
        {
            reference.Close();
            IsConnected = false;
        }
        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            reference.Dispose();
        }
    }
}
