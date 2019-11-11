using System;
using System.Runtime.InteropServices;

namespace HAL.DllImportMethods
{
    public abstract class BaseDLLImport : IDisposable
    {
        protected static readonly object key = new object();

        protected IntPtr ptr;

        public void Dispose()
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}