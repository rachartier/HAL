using System;

namespace client
{
    public class DemoClient : Client
    {
        public int Id {get;set;}

        public DemoClient(int id, string ip, int port) : base(ip, port)
        {
            Id = id;

            OnConnected += (o,e) => {
                StreamWriter.WriteLine(id);
            };
         }


        public override void Update()
        {
            try
            {
                string dataReceived = StreamReader.ReadLine();

                if (string.IsNullOrEmpty(dataReceived))
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