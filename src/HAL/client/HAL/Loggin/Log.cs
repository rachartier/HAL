using NLog;
using System.IO;

namespace HAL.Loggin
{
    public static class Log
    {
        public static readonly Logger Instance;

        static Log()
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Instance = LogManager.GetCurrentClassLogger();
        }
    }
}
