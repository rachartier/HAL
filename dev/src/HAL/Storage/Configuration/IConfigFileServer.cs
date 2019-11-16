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
        /// Get the maximum authorized connection for the server
        /// </summary>
        /// <returns>The maximum connection int if it exist, 100 otherwise</returns>
        public abstract int GetMaxConnection();

        /// <summary>
        /// Get the maximum retry for the retry-policy
        /// </summary>
        /// <returns>The maximum retry int if it exist, 3 otherwise</returns>
        public abstract int GetRetryMax();

        /// <summary>
        /// Get the path where to save the plugins sent to client
        /// </summary>
        /// <returns>The path string if it exist, null otherwise</returns>
        public abstract string GetPath();

        /// <summary>
        /// Get the name of the directory where the plugins are in the server
        /// </summary>
        /// <returns>The name string if it exist, null otherwise</returns>
        public abstract string GetPluginDirectory();
    }
}