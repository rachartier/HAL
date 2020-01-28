using HAL.CheckSum;
using HAL.Loggin;
using HAL.MagicString;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HAL.Connection.Client
{
    internal class HalClient : BaseClient
    {
        public event EventHandler<EventArgs> OnReceiveDone;

        private readonly FuncManager funcManager = new FuncManager();
        private readonly Stopwatch heartbeatStopwatch = new Stopwatch();
        private int stopwatchDelay = 10000;

        public HalClient(string ip, int port, int updateIntervalInMs = 100) : base(ip, port, updateIntervalInMs)
        {
            heartbeatStopwatch.Start();

            funcManager.AddFunc(MagicStringEnumerator.CMDAdd, FuncAdd);
            funcManager.AddFunc(MagicStringEnumerator.CMDDel, FuncDel);
            funcManager.AddFunc(MagicStringEnumerator.CMDUpd, FuncUpd);
            funcManager.AddFunc(MagicStringEnumerator.CMDHtb, FuncHtb);

            OnConnected += async (o, e) =>
            {
                if (!Directory.Exists(MagicStringEnumerator.DefaultPluginPath))
                {
                    Directory.CreateDirectory(MagicStringEnumerator.DefaultPluginPath);
                }
                var files = Directory.EnumerateFiles(MagicStringEnumerator.DefaultPluginPath);

                string configFileChecksum;

                try
                {
                    configFileChecksum = await CheckSumGenerator.HashOf(MagicStringEnumerator.DefaultConfigPath);
                }
                catch
                {
                    configFileChecksum = "0";
                }

                await StreamWriter.WriteLineAsync($"{MagicStringEnumerator.DefaultConfigPath};{configFileChecksum}");
                await StreamWriter.FlushAsync();

                foreach (var file in files)
                {
                    var checksum = await CheckSumGenerator.HashOf(file);

                    await StreamWriter.WriteLineAsync($"{file};{checksum}");
                    await StreamWriter.FlushAsync();
                }

                await StreamWriter.WriteLineAsync(MagicStringEnumerator.CMDEnd);
                await StreamWriter.FlushAsync();
            };
        }

        public override async Task UpdateAsync()
        {
            if (heartbeatStopwatch.ElapsedMilliseconds > stopwatchDelay)
            {
                StreamWriter.WriteLine(MagicStringEnumerator.CMDHtb);
                StreamWriter.Flush();
                heartbeatStopwatch.Restart();
            }
            string command = await StreamReader.ReadLineAsync();

            if (!string.IsNullOrEmpty(command))
            {
                var function = funcManager.GetFunc(command);

                if (function != null)
                    await function();
            }
        }

        private async Task FuncAdd()
        {
            string data = await StreamReader.ReadLineAsync();
            string[] args = data.Split(';', 2);

            string textBytesToRead = args[0];
            string path = args[1];

            int bytesToRead = int.Parse(textBytesToRead);

            char[] buffer = new char[bytesToRead];

            await StreamReader.ReadBlockAsync(buffer, 0, bytesToRead);

            string absolutePath = AppDomain.CurrentDomain.BaseDirectory + path;
            await File.WriteAllTextAsync(absolutePath, new string(buffer));

            Log.Instance?.Info($"File received from server: {absolutePath}");
        }

        private async Task FuncDel()
        {
            string path = await StreamReader.ReadLineAsync();

            File.Delete(path);

            Log.Instance?.Info($"File deleted by server: {path}");
        }

        private async Task FuncUpd()
        {
            OnReceiveDone?.Invoke(this, new EventArgs());
        }

        private async Task FuncHtb()
        {
        }
    }
}
