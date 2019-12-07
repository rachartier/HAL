namespace server
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