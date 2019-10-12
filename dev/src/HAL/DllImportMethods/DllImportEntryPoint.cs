using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HAL.DllImportMethods
{
    public class DllImportEntryPoint : BaseDLLImport
    {
        [DllImport("./lib/libreadso")]
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);

        /// <summary>
        /// run_entrypoint_sharedobject wrapper, to allocate the memory needed for the correct execution
        /// </summary>
        /// <param name="InputFile">dll/so path</param>
        /// <returns>the converted result string</returns>
        public string UseRunEntryPointSharedObject(string InputFile)
        {
            ptr = Marshal.StringToHGlobalAnsi(InputFile);
            IntPtr ptrResult;

            lock (key)
            {
                ptrResult = run_entrypoint_sharedobject(ptr);
            }

            // result need to be converted to a string type, otherwise it can't be read and memory will be corrupted
            return Marshal.PtrToStringAnsi(ptrResult);
        }
    }
}