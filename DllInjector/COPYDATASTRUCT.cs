using System;
using System.Runtime.InteropServices;

namespace DllInjector
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CopyDataStruct {
        public int cbData;
        public IntPtr dwData;
        public IntPtr lpData;
    }
}
