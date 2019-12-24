using HAL.CheckSum;
using HAL.Configuration;
using HAL.Loggin;
using HAL.MagicString;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HAL.Server
{
    internal class HalTcpClientSavedState : TcpClientSavedState
    {
        internal class MarkedChecksum
        {
            public string Checksum { get; set; }
            public bool Marked { get; set; }

            public MarkedChecksum(string checksum, bool marked = false)
            {
                Checksum = checksum;
                Marked = marked;
            }
        }
        private readonly IDictionary<string, MarkedChecksum> serverSidedFiles = new Dictionary<string, MarkedChecksum>();
        private readonly string savePath;
        private bool firstUpdate = true;

        public HalTcpClientSavedState(TcpClient client, string savePath)
            : base(client)
        {
            this.savePath = savePath;
        }

        public override async Task SaveAsync()
        {
            string result = await StreamReader.ReadLineAsync();

            if (string.IsNullOrEmpty(result))
                return;

            var splitedResult = result.Split(';', 3);

            string path = splitedResult[0];
            string filename = splitedResult[1];
            string content = splitedResult[2];

            string folder = $"{savePath}/{MagicStringEnumerator.RootSaveResults}/{path}/";

            Directory.CreateDirectory(folder);

            using var fw = File.CreateText($"{folder}{filename}");
            await fw.WriteAsync(content);
        }

        public override async Task FirstUpdateAsync()
        {
            while (true)
            {
                var textArgs = await StreamReader.ReadLineAsync();
                var args = textArgs.Split(';');

                if (args[0].Equals(MagicStringEnumerator.CMDEnd))
                    break;

                if (!serverSidedFiles.ContainsKey(args[0]))
                {
                    string fileName = args[0];

                    if(args[0] == MagicStringEnumerator.DefaultConfigPath)
                    {
                        fileName = MagicStringEnumerator.DefaultConfigPathServerToClient;
                    }

                    serverSidedFiles.Add(fileName, new MarkedChecksum(args[1]));
                }
            }
        }

        public override async Task UpdateAsync()
        {
            bool filesUpdated = false;

            var files = Directory.EnumerateFiles(MagicStringEnumerator.DefaultPluginPath);

            foreach (var entry in serverSidedFiles.Values)
            {
                entry.Marked = false;
            }

            foreach (var file in files)
            {
                var checksum = await CheckSumGenerator.HashOf(file);
                var code = await File.ReadAllTextAsync(file);

                if (!serverSidedFiles.ContainsKey(file))
                {
                    serverSidedFiles.Add(file, new MarkedChecksum("0"));
                }

                if (serverSidedFiles[file]?.Checksum.Equals(checksum) == false)
                {
                    filesUpdated = true;

                    string path = file;

                    if (file.Equals(MagicStringEnumerator.DefaultConfigPathServerToClient))
                        path = MagicStringEnumerator.DefaultConfigPath;
                    
                    serverSidedFiles[file].Checksum = checksum;

                    await SendAddCommand(code.Length, file, code);
                }

                serverSidedFiles[file].Marked = true;
            }

            if (filesUpdated || firstUpdate)
            {
                firstUpdate = false;
                await SendUpdateCommand();
            }

            /*
                        foreach(var key in pluginFile.Keys)
                        {
                            if(!pluginFile[key].Marked)
                            {
                                await StreamWriter.WriteLineAsync($"DEL\n{key}");
                                await StreamWriter.FlushAsync();

                                pluginFile.Remove(key);
                            }
                        }
            */
        }

        private async Task SendAddCommand(int length, string path, string code)
        {
            await StreamWriter.WriteLineAsync($"{MagicStringEnumerator.CMDAdd}\n{length};{path}\n{code}");
            await StreamWriter.FlushAsync();
        }
        private async Task SendUpdateCommand()
        {
            await StreamWriter.WriteLineAsync(MagicStringEnumerator.CMDUpd);
            await StreamWriter.FlushAsync();
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var configFile = new JSONConfigFileServer();
            configFile.Load(MagicStringEnumerator.DefaultConfigPath);

            string ip = configFile.GetAddress();
            string savePath = configFile.GetSavePath();
            int port = configFile.GetPort();
            int maxThreads = configFile.GetMaxThreads();
            int updateTimeMs = configFile.GetUpdateRate();

            if (string.IsNullOrEmpty(ip))
            {
                Log.Instance?.Error("You must provide 'ip' in the configuration file.");
                return;
            }

            Log.Instance?.Info($"Server ip: {ip}");
            Log.Instance?.Info($"Server port: {port}");
            Log.Instance?.Info($"Server update time in ms: {updateTimeMs}");
            Log.Instance?.Info($"Server threads count: {maxThreads}");

            var server = new BaseServer(ip, port, maxThreads, updateTimeMs);
            int connectedClients = 0;

            server.OnServerStarted += (o, e) =>
            {
                Log.Instance?.Info("Server starded, waiting for clients...");
            };

            server.OnClientConnected += (o, e) =>
            {
                connectedClients++;
                Log.Instance?.Info($"New client connected... (actual clients: {connectedClients})");
            };

            server.OnClientDisconnected += (o, e) =>
            {
                connectedClients--;
                Log.Instance?.Info($"A client has been disconnected: (actual clients: {connectedClients})");
            };

            server.StartUniqueClientType<HalTcpClientSavedState>(savePath);
        }
    }
}