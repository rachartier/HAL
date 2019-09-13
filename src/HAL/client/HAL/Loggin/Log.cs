﻿using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HAL.Loggin
{
    public static class Log
    {
        public static Logger Instance { get; private set; }

        static Log()
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Instance = LogManager.GetCurrentClassLogger();
        }
    }
}
