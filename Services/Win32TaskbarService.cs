using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AgyUsageShower.Services
{
    public static class Win32TaskbarService
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        public static void SetOverlayWindowStyles(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            long exStyle = (IntPtr.Size == 8)
                ? GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64()
                : GetWindowLong32(hwnd, GWL_EXSTYLE);

            exStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;

            if (IntPtr.Size == 8)
            {
                SetWindowLongPtr64(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
            }
            else
            {
                SetWindowLong32(hwnd, GWL_EXSTYLE, (int)exStyle);
            }
        }

        public static (double Left, double Top) CalculateRightDockPosition(double windowWidth, double windowHeight, Window window)
        {
            IntPtr taskbarHwnd = FindWindow("Shell_TrayWnd", null);
            double dpiScaleX = 1.0;
            double dpiScaleY = 1.0;

            PresentationSource source = PresentationSource.FromVisual(window);
            if (source != null && source.CompositionTarget != null)
            {
                dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            if (taskbarHwnd != IntPtr.Zero && GetWindowRect(taskbarHwnd, out RECT taskbarRect))
            {
                IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
                int rightEdgePixel = taskbarRect.Right;

                if (trayHwnd != IntPtr.Zero && GetWindowRect(trayHwnd, out RECT trayRect))
                {
                    rightEdgePixel = trayRect.Left - 10;
                }
                else
                {
                    rightEdgePixel = taskbarRect.Right - 150;
                }

                double targetLeftDip = (rightEdgePixel / dpiScaleX) - windowWidth;
                double targetTopDip = (taskbarRect.Top / dpiScaleY) + ((taskbarRect.Height / dpiScaleY - windowHeight) / 2.0);

                if (targetLeftDip < 0) targetLeftDip = 10;
                if (targetTopDip < 0) targetTopDip = 10;

                return (targetLeftDip, targetTopDip);
            }

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            return (screenWidth - windowWidth - 160, screenHeight - windowHeight - 10);
        }
    }
}
