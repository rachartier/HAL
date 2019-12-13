namespace HAL.Configuration
{
    public abstract class IConfigFileServer<TRoot, TToken> : IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
    {
        /// <summary>
        /// Get the port of the socket server
        /// </summary>
        /// <returns>The port int if it exist, 11000 otherwise</returns>
        public abstract int GetPort();

        /// <summary>
        /// Get the address wich will be running the server
        /// </summary>
        public abstract string GetAddress();

        /// <summary>
        /// Get the update rate in milliseconds
        /// </summary>
        public abstract int GetUpdateRate();

        /// <summary>
        /// Get the maximum used threads for the server
        /// </summary>
        /// <returns>The maximum connection int if it exist, max threads count of your processor otherwise</returns>
        public abstract int GetMaxThreads();
    }
}