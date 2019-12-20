﻿using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using HAL.CheckSum;
using HAL.Configuration;
using HAL.Loggin;
using HAL.MagicString;

namespace HAL.Server
{
    class HalTcpClientSavedState : TcpClientSavedState
    {
        internal class MarkedChecksum 
        {
            public string Checksum {get; set;}
            public bool Marked {get; set;}

            public MarkedChecksum(string checksum, bool marked = false)
            {
                Checksum = checksum;
                Marked = marked;
            }
        }
        private string savePath;
        private IDictionary<string, MarkedChecksum> pluginFile = new Dictionary<string, MarkedChecksum>();

        public HalTcpClientSavedState(TcpClient client, string savePath)
            : base(client)
        {
            this.savePath = savePath;
        }
        public override async Task SaveAsync()
        {
            string path = await StreamReader.ReadLineAsync();
            string filename = await StreamReader.ReadLineAsync();
            string content = await StreamReader.ReadLineAsync();
            
            if(string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(content))
                return;

            string folder = $"{savePath}/{MagicStringEnumerator.RootSaveResults}/{path}/";

            Directory.CreateDirectory(folder);

            using var fw = File.CreateText($"{folder}{filename}");
            await fw.WriteAsync(content);
        } 

        public override async Task FirstUpdateAsync() 
        {
            while(true)
            {
                var textArgs = await StreamReader.ReadLineAsync();
                var args = textArgs.Split(';');

                if(args[0].Equals(MagicStringEnumerator.CMDEnd))
                    break;
                
                if(!pluginFile.ContainsKey(args[0]))
                    pluginFile.Add(args[0], new MarkedChecksum(args[1]));
            }
        }

        public override async Task UpdateAsync()
        {
            bool filesUpdated = false;

            var files = Directory.EnumerateFiles(MagicStringEnumerator.DefaultPluginPath);

            foreach(var entry in pluginFile.Values) 
            {
                entry.Marked = false;
            }

            foreach(var file in files)
            {
                var checksum = await CheckSumGenerator.HashOf(file);
                var code = await File.ReadAllTextAsync(file);

                if(!pluginFile.ContainsKey(file))
                {
                    pluginFile.Add(file, new MarkedChecksum("0"));
                }

                if(pluginFile[file]?.Checksum.Equals(checksum) == false) 
                {
                    filesUpdated = true;

                    if(file.Equals(MagicStringEnumerator.DefaultConfigPathServerToClient))
                    {
                            await StreamWriter.WriteLineAsync($"{MagicStringEnumerator.CMDAdd}\n{code.Length}\n{MagicStringEnumerator.DefaultConfigPath}\n{code}\n");
                            await StreamWriter.FlushAsync();
                    }
                    else 
                    {
                        await StreamWriter.WriteLineAsync($"{MagicStringEnumerator.CMDAdd}\n{code.Length}\n{file}\n{code}\n");
                        await StreamWriter.FlushAsync();   
                    }

                    pluginFile[file].Checksum = checksum;                 
                }

                pluginFile[file].Marked = true;
            }

            if(filesUpdated)
            {
                await StreamWriter.WriteLineAsync(MagicStringEnumerator.CMDUpd);
                await StreamWriter.FlushAsync();
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
            configFile.Load(MagicStringEnumerator.DefaultConfigPath);

            string ip = configFile.GetAddress();
            string savePath = configFile.GetSavePath(); 
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

            server.StartUniqueClientType<HalTcpClientSavedState>(savePath);
        }
    }
}