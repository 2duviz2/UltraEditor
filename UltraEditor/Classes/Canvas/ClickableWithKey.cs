using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    public class ClickableWithKey : MonoBehaviour
    {
        public Button button;

        public void Update()
        {
            if (Input.GetMouseButtonDown(3)) // back button
                button.onClick.Invoke();
        }
    }
}
