using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    internal class DeactivateIfNotAdvanced : MonoBehaviour
    {
        public GameObject go;

        public void Update()
        {
            if (EditorManager.Instance != null)
            {
                go.SetActive(EditorManager.friendlyAdvancedInspector);
            }
        }
    }
}