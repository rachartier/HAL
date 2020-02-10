using System;
using System.Runtime.InteropServices;

namespace HAL.DllImportMethods
{
    public class DllImportEntryPoint : BaseDLLImport
    {
        [DllImport("./lib/libreadso")]
#pragma warning disable IDE1006
        private static extern IntPtr run_entrypoint_sharedobject(IntPtr input_file);

#pragma warning restore IDE1006

        /// <summary>
        ///     run_entrypoint_sharedobject wrapper, to allocate the memory needed for the correct execution
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