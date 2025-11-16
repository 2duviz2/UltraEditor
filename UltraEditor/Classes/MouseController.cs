using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace UltraEditor.Classes
{
    public static class MouseController
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
    }
}
