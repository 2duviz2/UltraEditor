using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    public class ClickableWithKey : MonoBehaviour
    {
        public KeyCode keyCode = KeyCode.Tilde;
        public Button button;

        public void Update()
        {
            if (Input.GetKeyDown(keyCode))
                button.onClick.Invoke();
        }
    }
}
