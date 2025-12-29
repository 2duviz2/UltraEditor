using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class OpenURLButton : MonoBehaviour
    {
        public string url;

        public void OpenURL()
        {
            Application.OpenURL(url);
        }
    }
}
