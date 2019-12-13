using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using HAL.CheckSum;

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
                        await StreamWriter.WriteLineAsync($"ADD\nconfig/config.json\n{code}\nEND");
                        await StreamWriter.FlushAsync();
                    }
                    else 
                    {
                        await StreamWriter.WriteLineAsync($"ADD\n{file}\n{code}\nEND");
                        await StreamWriter.FlushAsync();
                    }

                    pluginFile[file].Checksum = checksum;                 
                }

                pluginFile[file].Marked = true;
            }

            foreach(var key in pluginFile.Keys) 
            {
                if(!pluginFile[key].Marked) 
                {
                    await StreamWriter.WriteLineAsync($"DEL\n{key}\nEND");
                    await StreamWriter.FlushAsync();

                    pluginFile.Remove(key);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var server = new BaseServer("127.0.0.1", 1664, Environment.ProcessorCount);
            int connectedClients = 0;

            server.OnServerStarted += (o,e) => {
                Console.WriteLine("Server starded, waiting for clients...");
            };

            server.OnClientConnected += (o, e) => {
                connectedClients++;
                Console.WriteLine($"Clients: {connectedClients}");
            };

            server.OnClientDisconnected += (o, e) => {
                connectedClients--;
                Console.WriteLine($"Clients: {connectedClients}");
            };

            server.StartUniqueClientType<HalTcpClientSavedState>();
        }
    }
}