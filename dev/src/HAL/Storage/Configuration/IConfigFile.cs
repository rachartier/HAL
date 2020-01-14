namespace HAL.Configuration
{
    /// <summary>
    /// a configuration file is composed of a composite, root wil be the first to be read
    /// </summary>
    /// <typeparam name="TRoot">the root type</typeparam>
    /// <typeparam name="TToken">the token type</typeparam>
    public abstract class IConfigFile<TRoot, TToken>
        where TRoot : class
        where TToken : class
    {
        public TRoot Root { get; protected set; }

        /// <summary>
        /// load a configuration file
        /// </summary>
        /// <param name="file">configuration file path</param>
        public abstract void Load(string file);
    }
}
