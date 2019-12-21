using HAL.CheckSum;
using HAL.Loggin;
using HAL.MagicString;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HAL.Connection.Client
{
    internal class HalClient : BaseClient
    {
        public event EventHandler<EventArgs> OnReceiveDone;

        private readonly FuncManager funcManager = new FuncManager();

        public HalClient(string ip, int port, int updateIntervalInMs = 100) : base(ip, port, updateIntervalInMs)
        {
            funcManager.AddFunc(MagicStringEnumerator.CMDAdd, FuncAdd);
            funcManager.AddFunc(MagicStringEnumerator.CMDDel, FuncDel);
            funcManager.AddFunc(MagicStringEnumerator.CMDUpd, FuncUpd);

            OnConnected += async (o, e) =>
            {
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
            StringBuilder sb = new StringBuilder();
            string textBytesToRead = await StreamReader.ReadLineAsync();
            string path = await StreamReader.ReadLineAsync();

            int bytesToRead = int.Parse(textBytesToRead);

            while (bytesToRead > 0)
            {
                string line = await StreamReader.ReadLineAsync();
                bytesToRead -= line.Length + 1;

                sb.Append($"{line}\n");
            }

            await File.WriteAllTextAsync(path, sb.ToString());

            Log.Instance?.Info($"File received from server: {path}");
        }

        private async Task FuncDel()
        {
            string path = StreamReader.ReadLine();

            File.Delete(path);

            Log.Instance?.Info($"File deleted by server: {path}");
        }

        private async Task FuncUpd()
        {
            OnReceiveDone?.Invoke(this, new EventArgs());
        }
    }
}