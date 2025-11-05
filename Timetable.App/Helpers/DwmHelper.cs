using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Timetable.App.Helpers;

public static class DwmHelper
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    public static bool SetImmersiveDarkMode(Window window, bool enabled)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        int useDarkMode = enabled ? 1 : 0;
        int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        return result == 0;
    }
}