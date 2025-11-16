using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace UltraEditor.Classes
{
    internal class VersionText : MonoBehaviour
    {
        public TMP_Text text;

        void Start()
        {
            text.text = $"v{Plugin.GetVersion()}";
        }
    }
}