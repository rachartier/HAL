﻿using System;
using System.Runtime.InteropServices;

namespace HAL.DllImportMethods
{
    public class DllImportLaunchCmdUnix : BaseDLLImport
    {
        [DllImport("./lib/liblaunchcmdunix")]
#pragma warning disable IDE1006
        private static extern IntPtr launch_command(IntPtr command);

#pragma warning restore IDE1006

        /// <summary>
        ///     launch a command from the shell (unix)
        /// </summary>
        /// <param name="command">command to be executedpath</param>
        /// <returns>the command's result</returns>
        public string UseLaunchCommand(string command)
        {
            IntPtr ptrResult;
            lock (key)
            {
                ptr = Marshal.StringToHGlobalAnsi(command);
                ptrResult = launch_command(ptr);
            }

            return Marshal.PtrToStringAnsi(ptrResult);
        }
    }
}