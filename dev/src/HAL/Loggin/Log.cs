using NLog;
using System;
using HAL.MagicString;

namespace HAL.Loggin
{
    public static class Log
    {
        public static readonly Logger Instance;

        static Log()
        {
            try
            {
                LogManager.LoadConfiguration(MagicStringEnumerator.DefaultNLogConfigPath);
                Instance = LogManager.GetCurrentClassLogger();
            }
            catch (Exception)
            {
                //Console.Error.WriteLine("nlog.config not found. No loggin will be used.");
            }
        }
    }
}
