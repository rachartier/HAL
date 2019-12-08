using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace server
{
    class DemoTcpClientSavedState : TcpClientSavedState
    {
        private Random random = new Random();
        private int id;

        private string dataToSend = "coucou";

        public DemoTcpClientSavedState(TcpClient client)
            : base(client)
        {

        }

        public override async Task FirstUpdateAsync() 
        {
            int id = int.Parse(await StreamReader.ReadLineAsync());
            Console.WriteLine($"New client: #{id}");
        }

        public override async Task UpdateAsync()
        {
            await StreamWriter.WriteLineAsync(dataToSend);
            await StreamWriter.FlushAsync();

            Thread.Sleep((1 + random.Next(5)) * 1000);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server("127.0.0.1", 1664, Environment.ProcessorCount);
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

            server.StartUniqueClientType<DemoTcpClientSavedState>();
        }
    }
}