namespace UltraEditor.Classes;

using System.Runtime.InteropServices;

public static class MouseController
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    public static (int x, int y) GetMousePos()
    {
        GetCursorPos(out POINT p);
        return (p.X, p.Y);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int X;
        public int Y;
    }
}