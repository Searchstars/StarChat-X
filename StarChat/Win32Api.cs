//由人类赛博科技灵魂集合体：ChatGPT编写

using System;
using System.Runtime.InteropServices;

public class Win32Api
{
    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern bool ImmSetCompositionWindow(IntPtr hImc, ref COMPOSITIONFORM lpCompForm);

    [DllImport("imm32.dll")]
    public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hImc);

    [DllImport("CHxReadingStringIME.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool ShowReadingWindow(IntPtr hwnd, bool bShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMPOSITIONFORM
    {
        public uint dwStyle;
        public POINT ptCurrentPos;
        public RECT rcArea;
    }

    public const uint CFS_POINT = 0x0002;
}