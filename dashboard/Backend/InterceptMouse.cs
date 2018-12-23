using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;

namespace HIO.Backend
{
    class InterceptMouse
    {
        /// ES_PASSWORD -> 0x0020L
        public const int ES_PASSWORD = 32;
        public static int textType = 0; //0 other ,1 textbox,2 password
        enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out Point lpPoint);
        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();
        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;
        public void run()
        {
            _hookID = SetHook(_proc);
            // UnhookWindowsHookEx(_hookID);
        }
        public int checkClick()
        {
            return textType;
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {

            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)
            {

                return SetWindowsHookEx(WH_MOUSE_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }

        }


        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);


        private static IntPtr HookCallback(

            int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                IntPtr hWnd = WindowFromPoint(hookStruct.pt);
                if (hWnd != IntPtr.Zero)
                {
                    StringBuilder szClassName = new StringBuilder(256);
                    int nRet;
                    nRet = GetClassName(hWnd, szClassName, szClassName.Capacity);

                    if (nRet == 0) return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    Debug.Write(szClassName.ToString());
                    if (szClassName.ToString() == "Edit" || szClassName.ToString() == "TEdit" || szClassName.ToString() == "ThunderTextBox")
                    {
                        textType = 1;
                        long ip = (long)GetWindowLong(hWnd, (int)WindowLongFlags.GWL_STYLE);
                        ip &= ES_PASSWORD;
                        if (ip == ES_PASSWORD)
                            textType = 2;
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }


                }
                //Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);

            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        }


        private const int WH_MOUSE_LL = 14;


        private enum MouseMessages
        {

            WM_LBUTTONDOWN = 0x0201,

            WM_LBUTTONUP = 0x0202,

            WM_MOUSEMOVE = 0x0200,

            WM_MOUSEWHEEL = 0x020A,

            WM_RBUTTONDOWN = 0x0204,

            WM_RBUTTONUP = 0x0205

        }


        [StructLayout(LayoutKind.Sequential)]

        private struct POINT
        {

            public int x;

            public int y;

        }


        [StructLayout(LayoutKind.Sequential)]

        private struct MSLLHOOKSTRUCT
        {

            public POINT pt;

            public uint mouseData;

            public uint flags;

            public uint time;

            public IntPtr dwExtraInfo;

        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook,

            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

            IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
