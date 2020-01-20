using HAL.MagicString;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HAL.Server
{
    public abstract class TcpClientSavedState : IDisposable
    {
        public readonly StreamWriter StreamWriter;
        public readonly StreamReader StreamReader;

        public readonly Stopwatch Stopwatch;
        public readonly Stopwatch HeartbeatStopwatch;

        public bool IsConnected { get; set; }
        public bool IsFirstUpdate { get; set; } = true;

        public readonly TcpClient reference;
        public readonly int HeartbeatUpdateTimeMs = 6000;
        public TcpClientSavedState(TcpClient client)
        {
            StreamWriter = new StreamWriter(client.GetStream());
            StreamReader = new StreamReader(client.GetStream());

            Stopwatch = new Stopwatch();
            HeartbeatStopwatch = new Stopwatch();

            Stopwatch.Start();
            HeartbeatStopwatch.Start();

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
            StreamWriter.Flush();
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
