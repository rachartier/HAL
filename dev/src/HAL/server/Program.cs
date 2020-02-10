using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using HAL.CheckSum;
using HAL.Configuration;
using HAL.Loggin;
using HAL.MagicString;

namespace HAL.Server
{
    internal class HalTcpClientSavedState : TcpClientSavedState
    {
        private readonly string savePath;

        private readonly IDictionary<string, MarkedChecksum>
            serverSidedFiles = new Dictionary<string, MarkedChecksum>();

        private bool firstUpdate = true;

        public HalTcpClientSavedState(TcpClient client, string savePath)
            : base(client)
        {
            this.savePath = savePath;
        }

        private string ConvertUriAbsoluteToLocal(string uri)
        {
            char[] splitDelimiters = {'\\', '/'};
            return uri.Split(splitDelimiters).Last();
        }

        public override async Task SaveAsync()
        {
            if (reference.Available <= 0) return;

            var result = await StreamReader.ReadLineAsync();

            if (string.IsNullOrEmpty(result))
                return;

            var splitedResult = result.Split(';', 3);

            var path = splitedResult[0];
            var filename = splitedResult[1];
            var content = splitedResult[2];

            var folder = $"{savePath}/{MagicStringEnumerator.RootSaveResults}/{path}/";
            Directory.CreateDirectory(folder);

            using var fw = File.CreateText($"{folder}{filename}");
            await fw.WriteAsync(content);

            //Log.Instance?.Info($"{path} {filename} {content}");
        }

        public override async Task FirstUpdateAsync()
        {
            while (true)
            {
                var textArgs = await StreamReader.ReadLineAsync();
                var args = textArgs.Split(';');

                if (args[0].Equals(MagicStringEnumerator.CMDEnd))
                    break;

                var fileName = ConvertUriAbsoluteToLocal(args[0]);

                if (!serverSidedFiles.ContainsKey(fileName))
                {
                    if (args[0] == MagicStringEnumerator.DefaultConfigPath)
                        fileName = MagicStringEnumerator.DefaultConfigPathServerToClient;

                    serverSidedFiles.Add(fileName, new MarkedChecksum(args[1]));
                }
            }
        }

        public override async Task UpdateAsync()
        {
            var filesUpdated = false;

            var files = Directory.EnumerateFiles(MagicStringEnumerator.DefaultPluginPath);

            foreach (var entry in serverSidedFiles.Values) entry.Marked = false;

            foreach (var file in files)
            {
                var checksum = await CheckSumGenerator.HashOf(file);
                var code = await File.ReadAllTextAsync(file);

                var fileName = ConvertUriAbsoluteToLocal(file);

                if (!serverSidedFiles.ContainsKey(fileName)) serverSidedFiles.Add(fileName, new MarkedChecksum("0"));

                Console.WriteLine(fileName);

                if (serverSidedFiles[fileName]?.Checksum.Equals(checksum) == false)
                {
                    filesUpdated = true;

                    string path;

                    if (file.Equals(MagicStringEnumerator.DefaultConfigPathServerToClient))
                        path = MagicStringEnumerator.DefaultRelativeConfigPath + fileName;
                    else
                        path = MagicStringEnumerator.DefaultRelativePluginPath + fileName;

                    serverSidedFiles[fileName].Checksum = checksum;

                    await SendAddCommand(code.Length, path, code);
                }

                serverSidedFiles[fileName].Marked = true;
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

        internal class MarkedChecksum
        {
            public MarkedChecksum(string checksum, bool marked = false)
            {
                Checksum = checksum;
                Marked = marked;
            }

            public string Checksum { get; set; }
            public bool Marked { get; set; }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var configFile = new JSONConfigFileServer();
            try
            {
                configFile.Load(MagicStringEnumerator.DefaultConfigPath);
                Log.Instance?.Info($"Configuration file {MagicStringEnumerator.DefaultConfigPath} loaded");
            }
            catch (Exception ex)
            {
                Log.Instance?.Error($"{MagicStringEnumerator.DefaultConfigPath}: {ex.Message}");
                Log.Instance?.Error("Program closed due to errors.");
                return;
            }

            var ip = configFile.GetAddress();
            var savePath = configFile.GetSavePath();
            var port = configFile.GetPort();
            var maxThreads = configFile.GetMaxThreads();
            var updateTimeMs = configFile.GetUpdateRate();

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

            server.OnServerStarted += (o, e) =>
            {
                Log.Instance?.Info("Server starded, waiting for clients...");

                AppDomain.CurrentDomain.ProcessExit += (o, e) =>
                {
                    server.Stop();
                    Log.Instance?.Error("Unexcepted program exit.");
                };
            };

            server.OnClientConnected += (o, e) =>
            {
                Log.Instance?.Info($"New client connected... (actual clients: {server.ClientsCount})");
            };

            server.OnClientDisconnected += (o, e) => { Log.Instance?.Info("A client has been disconnected."); };

            server.StartUniqueClientType<HalTcpClientSavedState>(savePath);
        }
    }
}