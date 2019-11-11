namespace HAL.Configuration
{
    public abstract class IConfigFileServer<TRoot, TToken> : IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
    {
    }
}