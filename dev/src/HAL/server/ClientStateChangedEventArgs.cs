namespace HAL.Server
{
    public class ClientStateChangedEventArgs
    {
        public readonly TcpClientSavedState Client;

        public ClientStateChangedEventArgs(TcpClientSavedState client)
        {
            Client = client;
        }
    }
}