using System.Runtime.InteropServices;

namespace DllInjector
{
    public class WindowMessage
    {
        public byte[] Data { get; }
        public int Type { get; }

        public WindowMessage(CopyDataStruct cps)
        {
            Type = cps.cbData;
            Data = new byte[(int)cps.dwData];
            Marshal.Copy(cps.lpData, Data, 0, (int)cps.dwData);
        }
    }
}
