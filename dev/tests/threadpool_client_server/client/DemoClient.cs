using System;
using System.Threading.Tasks;

namespace client
{
    public class DemoClient : Client
    {
        public int Id { get; set; }

        public DemoClient(int id, string ip, int port) : base(ip, port)
        {
            Id = id;

            OnConnected += async (o, e) =>
            {
                await StreamWriter.WriteLineAsync($"{id}");
                await StreamWriter.FlushAsync();
            };
        }

        public override async Task UpdateAsync()
        {
            try
            {
                string result = StreamReader.ReadLine();
                Console.WriteLine(result);

                if (string.IsNullOrEmpty(result))
                {
                    IsConnected = false;
                }
            }
            catch
            {
                IsConnected = false;
            }
        }
    }
}