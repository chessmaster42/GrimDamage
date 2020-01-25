using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DllInjector
{
    /// <summary>
    /// 
    /// </summary>
    public static class DllInjector {        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DllInjector));

        public static HashSet<uint> FindProcessForWindow(string windowname) {
            HashSet<uint> clients = new HashSet<uint>();
            IntPtr prevWindow = IntPtr.Zero;
            do {
                prevWindow = Win32.FindWindowEx(IntPtr.Zero, prevWindow, windowname, null);
                if (prevWindow != IntPtr.Zero) {
                    Win32.GetWindowThreadProcessId(prevWindow, out uint pid);
                    clients.Add(pid);
                }
            } while (prevWindow != IntPtr.Zero);

            return clients;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nProcessId"></param>
        /// <param name="sDllPath"></param>
        /// <returns></returns>
        public static IntPtr NewInject(uint nProcessId, string sDllPath)
        {
            IntPtr hndProc = Win32.OpenProcess(Win32.PROCESS_CREATE_THREAD | Win32.PROCESS_VM_OPERATION | Win32.PROCESS_VM_READ | Win32.PROCESS_VM_WRITE | Win32.PROCESS_QUERY_INFORMATION, 0, nProcessId);

            if (Path.GetFileName(sDllPath)?.Equals(sDllPath) == true)
                throw new ArgumentException("The DLL path must be an absolute path to DLL");
            try
            {
                IntPtr remoteModule = LoadLibraryEx(hndProc, sDllPath);
                return remoteModule;
            }
            finally
            {
                Win32.CloseHandle(hndProc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nProcessId"></param>
        /// <param name="remoteModule"></param>
        /// <returns></returns>
        public static bool UnloadDll(uint nProcessId, IntPtr remoteModule)
        {
            IntPtr hndProc = Win32.OpenProcess(Win32.PROCESS_CREATE_THREAD | Win32.PROCESS_VM_OPERATION | Win32.PROCESS_VM_READ | Win32.PROCESS_VM_WRITE | Win32.PROCESS_QUERY_INFORMATION, 0, nProcessId);
            try
            {
                return FreeLibraryEx(hndProc, remoteModule);
            }
            finally
            {
                Win32.CloseHandle(hndProc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="hRemoteModule"></param>
        /// <returns></returns>
        private static bool FreeLibraryEx(IntPtr hProcess, IntPtr hRemoteModule) {
            uint dwResult = 0;
            IntPtr hKernel32 = Win32.GetModuleHandle("kernel32.dll");
            if (hKernel32 != IntPtr.Zero) {
                IntPtr dwFreeLibrary = Win32.GetProcAddress(hKernel32, "FreeLibrary");
                if (dwFreeLibrary != IntPtr.Zero) {
                    IntPtr dwThreadId = IntPtr.Zero;
                    IntPtr hThread = Win32.CreateRemoteThread(
                        hProcess, 
                        IntPtr.Zero, 
                        IntPtr.Zero, 
                        dwFreeLibrary, 
                        hRemoteModule, 
                        0,
                        dwThreadId
                    );

                    if (hThread != IntPtr.Zero) {
                        uint dwWaitResult = Win32.WaitForSingleObject(hThread, 0xFFFFFFFF);
                        if (dwWaitResult == 0)
                            Win32.GetExitCodeThread(hThread, out dwResult);

                        Win32.CloseHandle(hThread);
                    }
                }
            }

            return dwResult != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="sModule"></param>
        /// <returns></returns>
        private static IntPtr LoadLibraryEx(IntPtr hProcess, string sModule) {
            uint dwResult = 0;
            IntPtr hKernel32 = Win32.GetModuleHandle("kernel32.dll");
            if (hKernel32 != IntPtr.Zero) {
                IntPtr dwLoadLibrary = Win32.GetProcAddress(hKernel32, "LoadLibraryW");
                if (dwLoadLibrary != IntPtr.Zero) {
                    byte[] bBuffer = Encoding.Unicode.GetBytes(sModule);
                    uint dwBufferLength = (uint)bBuffer.Length + 2;
                    IntPtr dwCodeCave = Allocate(hProcess, dwBufferLength);
                    if (dwCodeCave != IntPtr.Zero) {
                        if (Write(hProcess, dwCodeCave, sModule, CharSet.Unicode)) {
                            IntPtr dwThreadId = IntPtr.Zero;
                            IntPtr hThread = Win32.CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, dwLoadLibrary, dwCodeCave, 0, dwThreadId);

                            if (hThread != IntPtr.Zero) {
                                uint dwWaitResult = Win32.WaitForSingleObject(hThread, 0xFFFFFFFF);
                                if (dwWaitResult == 0)
                                    Win32.GetExitCodeThread(hThread, out dwResult);

                                Win32.CloseHandle(hThread);
                            }
                        }

                        if (!Free(hProcess, dwCodeCave))
                        {
                            Logger.Warn($"Failed to free memory for process {hProcess} at address {dwCodeCave}");
                        }
                    }
                }
            }

            return (IntPtr)dwResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="dwSize"></param>
        /// <returns></returns>
        private static IntPtr Allocate(IntPtr hProcess, uint dwSize)
        {
            return Win32.VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)dwSize, Win32.MEM_COMMIT | Win32.MEM_RESERVE, Win32.PAGE_EXECUTE_READWRITE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="dwAddress"></param>
        /// <returns></returns>
        private static bool Free(IntPtr hProcess, IntPtr dwAddress) {
            return Win32.VirtualFreeEx(hProcess, dwAddress, 0, Win32.FreeType.Release);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="dwAddress"></param>
        /// <param name="sString"></param>
        /// <param name="sCharacterSet"></param>
        /// <returns></returns>
        private static bool Write(IntPtr hProcess, IntPtr dwAddress, string sString, CharSet sCharacterSet) {
            byte[] bBuffer;
            if (sCharacterSet == CharSet.None || sCharacterSet == CharSet.Ansi)
                bBuffer = Encoding.ASCII.GetBytes(sString);
            else
                bBuffer = Encoding.Unicode.GetBytes(sString);

            uint dwStringLength = (uint)bBuffer.Length;

            bool bResult = Win32.WriteProcessMemory(hProcess, dwAddress, bBuffer, dwStringLength, out int dwBytesWritten) != 0;

            return bResult && dwBytesWritten == dwStringLength;
        }
    }
}
