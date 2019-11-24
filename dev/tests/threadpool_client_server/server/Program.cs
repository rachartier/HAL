using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace server
{
    class SimpleTcpClientSavedState : TcpClientSavedState
    {
        private Random random = new Random();
        private int id;

        private string dataToSend = "coucou";

        public SimpleTcpClientSavedState(TcpClient client)
            : base(client)
        {
            id = int.Parse(StreamReader.ReadLine());

            //Console.WriteLine($"New client: #{id}");
        }

        public override void Update()
        {
            StreamWriter.WriteLine(dataToSend);
            StreamWriter.Flush();

            Thread.Sleep((1 + random.Next(5)) * 1000);
        }
    }

    class Server
    {
        private TcpListener server;
        private bool isRunning = true;

        public Server(string ip, int port)
        {
            var connectionManager = new ThreadedConnectionManager(8, 100);

            server = new TcpListener(System.Net.IPAddress.Parse(ip), port);
            server.Start();

            Console.WriteLine("Server is running. Waiting for clients...");

            new Thread(() =>
            {
                while (true)
                {
                    connectionManager._Info();
                    Console.WriteLine("\nPress any key to kill all connections...");

                    Thread.Sleep(100);
                }
            }).Start();


            var mainThread = new Thread(async () =>
            {
                while (isRunning)
                {
                    var client = await server.AcceptTcpClientAsync();
                    var tcpOpenedStream = new SimpleTcpClientSavedState(client);

                    connectionManager.AddTcpClient(tcpOpenedStream);
                }
            });

            mainThread.Start();

            Console.ReadKey();
            connectionManager.KillAllConnections();

            mainThread.Join();
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server("127.0.0.1", 1664);
        }
    }
}