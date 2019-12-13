using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HAL.CheckSum;
using HAL.Loggin;

namespace HAL.Connection.Client
{
    class HalClient : BaseClient
    {
        public event EventHandler<EventArgs> OnReceiveDone;

        private readonly FuncManager funcManager = new FuncManager();

        public HalClient(string ip, int port, int updateIntervalInMs = 100) : base(ip, port, updateIntervalInMs)
        {
            funcManager.AddFunc("ADD", FuncAdd);
            funcManager.AddFunc("DEL", FuncDel);
        
            OnConnected += async(o, e) =>
            {
                var files = Directory.EnumerateFiles("plugins/");

                foreach (var file in files)
                {
                    var checksum = CheckSumGenerator.HashOf(file);

                    await StreamWriter.WriteLineAsync($"{file};{checksum}");
                    await StreamWriter.FlushAsync();
                }

                await StreamWriter.WriteLineAsync($"END");
                await StreamWriter.FlushAsync();

                OnReceiveDone?.Invoke(this, new EventArgs());
            };
        }

        public override async Task UpdateAsync()
        {
            string command = StreamReader.ReadLine();

            if(!string.IsNullOrEmpty(command))
            {
                var function = funcManager.GetFunc(command);
                
                if(function != null)
                    await function();
            }
        }

        private async Task FuncAdd()
        {
            StringBuilder sb = new StringBuilder();
            string textBytesToRead = StreamReader.ReadLine();
            string path = StreamReader.ReadLine();

            int bytesToRead = int.Parse(textBytesToRead);

            while (bytesToRead > 0)
            {
                string line = StreamReader.ReadLine();
                bytesToRead -= line.Length + 1;

                sb.Append($"{line}\n");
            }

            await File.WriteAllTextAsync(path, sb.ToString());

            Log.Instance?.Info($"File received from server: {path}");
            OnReceiveDone?.Invoke(this, new EventArgs());
        }

        private async Task FuncDel()
        {
            string path = StreamReader.ReadLine();

            File.Delete(path);

            Log.Instance?.Info($"File deleted by server: {path}");
            OnReceiveDone?.Invoke(this, new EventArgs());
        }
    }
}