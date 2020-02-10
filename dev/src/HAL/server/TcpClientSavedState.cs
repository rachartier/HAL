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
        public readonly Stopwatch HeartbeatStopwatch;
        public readonly int HeartbeatUpdateTimeMs = 6000;

        public readonly TcpClient reference;

        public readonly Stopwatch Stopwatch;
        public readonly StreamReader StreamReader;
        public readonly StreamWriter StreamWriter;

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

        public bool IsConnected { get; set; }
        public bool IsFirstUpdate { get; set; } = true;

        public void Dispose()
        {
            StreamWriter.Dispose();
            StreamReader.Dispose();

            reference.Dispose();
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
    }
}