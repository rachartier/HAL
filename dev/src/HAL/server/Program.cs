using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using HAL.CheckSum;
using HAL.Configuration;
using HAL.Loggin;

namespace HAL.Server
{
    class HalTcpClientSavedState : TcpClientSavedState
    {
        class MarkedChecksum 
        {
            public string Checksum {get; set;}
            public bool Marked {get; set;}

            public MarkedChecksum(string checksum, bool marked = false)
            {
                Checksum = checksum;
                Marked = marked;
            }
        }
        private IDictionary<string, MarkedChecksum> pluginFile = new Dictionary<string, MarkedChecksum>();

        public HalTcpClientSavedState(TcpClient client)
            : base(client)
        {

        }

        public override async Task FirstUpdateAsync() 
        {
            string[] fileArgs = null;

            while(true)
            {
                fileArgs = StreamReader.ReadLine().Split(';');
                
                if(fileArgs[0].Equals("END"))
                    break;

                pluginFile.Add(fileArgs[0], new MarkedChecksum(fileArgs[1]));
            }
        }
        public override async Task UpdateAsync()
        {
            var files = Directory.EnumerateFiles("plugins/");

            foreach(var entry in pluginFile.Values) 
            {
                entry.Marked = false;
            }

            foreach(var file in files) 
            {
                var checksum = CheckSumGenerator.HashOf(file);
                var code = File.ReadAllText(file);

                if(!pluginFile.ContainsKey(file))
                {
                    pluginFile.Add(file, new MarkedChecksum("0"));
                }

                if(pluginFile[file]?.Checksum.Equals(checksum) == false) 
                {
                    if(file.Equals("plugins/config.json"))
                    {
                        if(pluginFile["config/config.json"]?.Checksum.Equals(checksum) == false) 
                        {
                            await StreamWriter.WriteLineAsync($"ADD\n{code.Length}\nconfig/config.json\n{code}\n");
                            await StreamWriter.FlushAsync();
                        }
                    }
                    else 
                    {
                        await StreamWriter.WriteLineAsync($"ADD\n{code.Length}\n{file}\n{code}\n");
                        await StreamWriter.FlushAsync();
                    }

                    pluginFile[file].Checksum = checksum;                 
                }

                pluginFile[file].Marked = true;
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
    }

    class Program
    {
        static void Main(string[] args)
        {
            var configFile = new JSONConfigFileServer();
            configFile.Load("config/config.json");

            string ip = configFile.GetAddress();
            int port = configFile.GetPort();
            int maxThreads = configFile.GetMaxThreads();
            int updateTimeMs = configFile.GetUpdateRate();

            if(string.IsNullOrEmpty(ip))
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

            server.OnServerStarted += (o,e) => {
                Log.Instance?.Info("Server starded, waiting for clients...");
            };

            server.OnClientConnected += (o, e) => {
                connectedClients++;
                Log.Instance?.Info($"New client connected... (actual clients: {connectedClients})");
            };

            server.OnClientDisconnected += (o, e) => {
                connectedClients--;
                Log.Instance?.Info($"A client has been disconnected: (actual clients: {connectedClients})");
            };

            server.StartUniqueClientType<HalTcpClientSavedState>();
        }
    }
}