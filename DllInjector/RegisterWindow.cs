using log4net;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace DllInjector
{
    /// <summary>
    /// 
    /// </summary>
    public class RegisterWindow : IDisposable
    {
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changewindowmessagefilterex#parameters
        private enum ChangeWindowMessageFilterExAction : uint
        {
            Allow = 1
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static readonly ILog Logger = LogManager.GetLogger(typeof(RegisterWindow));
        private const uint WM_COPYDATA = 0x004A;
        private const int ERROR_CLASS_ALREADY_EXISTS = 1410;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly WndProc _wndProcDelegate;  // Must be class field to maintain delegate reference
        private Action<WindowMessage> _customWndProc;
        private IntPtr _hwnd;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="callback"></param>
        public RegisterWindow(string className, Action<WindowMessage> callback)
        {
            if (className == null) throw new Exception("className is null");
            if (className == string.Empty) throw new Exception("className is empty");

            _customWndProc = callback;

            _wndProcDelegate = ProxyWndProc;

            WndClassStruct wndClass = new WndClassStruct
            {
                lpszClassName = className,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
            };

            ushort classAtom = RegisterClassW(ref wndClass);

            int lastError = Marshal.GetLastWin32Error();

            if (classAtom == 0 && lastError != ERROR_CLASS_ALREADY_EXISTS)
            {
                throw new Exception("Could not register window class");
            }

            _hwnd = CreateWindowExW(
                0,
                className,
                string.Empty,
                0,
                0,
                0,
                0,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );
            Logger.Info("Created window " + _hwnd);

            if (!ChangeWindowMessageFilterEx(_hwnd, WM_COPYDATA, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero))
            {
                var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
                Logger.Warn("Failed to remove UIPI filter restrictions.");
                if (isAdmin)
                    Logger.Fatal("Running as administrator, will not be able to communicate with GD due to UIPI filter.");
                else
                    Logger.Info("Not running as administrator, UIPI filter not required.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _customWndProc = null;

            // Dispose unmanaged resources
            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr ProxyWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (msg == WM_COPYDATA)
                {
                    CopyDataStruct cps = (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));

                    WindowMessage message = new WindowMessage(cps);
                    _customWndProc?.Invoke(message);
                }

                return DefWindowProcW(hWnd, msg, wParam, lParam);
            }
            catch (Exception e)
            {
                Logger.Error("Something broke while decoding a window message", e);
                throw;
            }
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changewindowmessagefilterex
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, IntPtr passNull);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClassW([In] ref WndClassStruct lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
    }
}
