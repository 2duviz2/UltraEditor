using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class PrintAllComponents : MonoBehaviour
    {
        public TMP_Text text;
        public string prefix;

        public void Start()
        {
            text.text = prefix + string.Join(
            "<br><br>",
            EditorManager.GetAllMonoBehaviourTypes(true).Select(t => t.Name)
        );

        }
    }
}
