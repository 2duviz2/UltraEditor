using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    internal class ResetScrollbar : MonoBehaviour
    {
        public float value = 1;

        void Start()
        {
            GetComponent<Scrollbar>().value = value;
        }
    }
}
