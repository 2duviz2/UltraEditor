using System;
using System.Collections.Generic;
using System.Text;

namespace UltraEditor.Libraries
{
    public static class TextPatcher
    {
        static List<(string, string)> patches = [
            ("<b>", "<b><color=white>"),
            ("</b>", "</b></color>"),
            ];

        /// <summary> Returns a copy of the string with useful patches for text </summary>
        public static string Patch(string originalText)
        {
            string t = originalText;
            foreach (var pat in patches)
                t.Replace(pat.Item1, pat.Item2);
            return t;
        }
    }
}
