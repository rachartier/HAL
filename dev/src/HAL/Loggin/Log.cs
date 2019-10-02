using NLog;
using System;
using System.IO;

namespace HAL.Loggin
{
    public static class Log
    {
        public static readonly Logger Instance;

        static Log()
        {
            try
            {
                LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
                Instance = LogManager.GetCurrentClassLogger();
            }
            catch (Exception)
            {
                Console.Error.WriteLine("nlog.config not found. No loggin will be used.");
            }
        }
    }
}