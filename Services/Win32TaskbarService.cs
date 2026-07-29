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

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;

        private static int _lastX = -9999;
        private static int _lastY = -9999;
        private static bool _isParentSet = false;

        public static void HideFromAltTab(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            int exStyle = GetWindowLong32(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong32(hwnd, GWL_EXSTYLE, exStyle);
        }

        public static bool EmbedInsideTaskbar(Window window, double widthDip, double heightDip)
        {
            IntPtr windowHwnd = new WindowInteropHelper(window).Handle;
            if (windowHwnd == IntPtr.Zero) return false;

            IntPtr taskbarHwnd = FindWindow("Shell_TrayWnd", null);
            if (taskbarHwnd == IntPtr.Zero) return false;

            // Set parent to Shell_TrayWnd ONLY ONCE to prevent flickering
            if (!_isParentSet)
            {
                SetParent(windowHwnd, taskbarHwnd);
                int style = GetWindowLong32(windowHwnd, GWL_STYLE);
                style = (style & ~0x8000000) | WS_CHILD | WS_VISIBLE;
                SetWindowLong32(windowHwnd, GWL_STYLE, style);

                int exStyle = GetWindowLong32(windowHwnd, GWL_EXSTYLE);
                exStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                SetWindowLong32(windowHwnd, GWL_EXSTYLE, exStyle);

                _isParentSet = true;
            }

            // Calculate DPI scaling
            double dpiScaleX = 1.0;
            double dpiScaleY = 1.0;
            PresentationSource source = PresentationSource.FromVisual(window);
            if (source?.CompositionTarget != null)
            {
                dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            int pixelWidth = (int)(widthDip * dpiScaleX);
            int pixelHeight = (int)(heightDip * dpiScaleY);

            if (GetWindowRect(taskbarHwnd, out RECT taskbarRect))
            {
                IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
                int targetX = taskbarRect.Width - pixelWidth - 140;

                if (trayHwnd != IntPtr.Zero && GetWindowRect(trayHwnd, out RECT trayRect))
                {
                    POINT pt = new POINT { X = trayRect.Left, Y = trayRect.Top };
                    ScreenToClient(taskbarHwnd, ref pt);
                    targetX = pt.X - pixelWidth - 10;
                }

                int targetY = (taskbarRect.Height - pixelHeight) / 2;
                if (targetY < 0) targetY = 2;

                // Move ONLY IF position actually changed! Avoids DWM redraw flickering.
                if (targetX != _lastX || targetY != _lastY)
                {
                    _lastX = targetX;
                    _lastY = targetY;
                    SetWindowPos(windowHwnd, IntPtr.Zero, targetX, targetY, pixelWidth, pixelHeight, SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                }
                return true;
            }

            return false;
        }
    }
}
