using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace server
{
    class SimpleTcpClientOpenedStream : TcpClientOpenedStream
    {
        private Random random = new Random();
        private int id;

        private string dataToSend = "coucou";

        public SimpleTcpClientOpenedStream(TcpClient client)
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
        private bool isRunning;
        private object key = new object();

        public Server(string ip, int port)
        {
            var connectionManager = new ThreadedConnectionManager(8);

            server = new TcpListener(System.Net.IPAddress.Parse(ip), port);
            server.Start();

            Console.WriteLine("Server is running. Waiting for clients...");

            isRunning = true;


            new Thread(() =>
            {
                while (true)
                {
                    connectionManager._Info();
                    Thread.Sleep(1000);
                }
            }).Start();


            var mainThread = new Thread(async () =>
            {
                while (isRunning)
                {
                    var client = await server.AcceptTcpClientAsync();
                    var tcpOpenedStream = new SimpleTcpClientOpenedStream(client);

                    connectionManager.AddTcpClient(tcpOpenedStream);
                }
            });

            mainThread.Start();
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