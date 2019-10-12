using System;
using System.Runtime.InteropServices;

namespace HAL.DllImportMethods
{
    public class DllImportLaunchCmdUnix : BaseDLLImport
    {
        [DllImport("./lib/liblaunchcmdunix")]
        private static extern IntPtr launch_command(IntPtr command);

        /// <summary>
        /// launch a command from the shell (unix)
        /// </summary>
        /// <param name="command">command to be executedpath</param>
        /// <returns>the command's result</returns>
        public string UseLaunchCommand(string command)
        {
            lock (key)
            {
                ptr = Marshal.StringToHGlobalAnsi(command);
                IntPtr ptrResult = launch_command(ptr);

                return Marshal.PtrToStringAnsi(ptrResult);
            }
        }
    }
}