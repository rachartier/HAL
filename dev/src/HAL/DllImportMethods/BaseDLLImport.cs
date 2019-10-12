using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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