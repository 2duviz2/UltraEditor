using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class DeactivateIfNonEditable : MonoBehaviour
    {
        public GameObject go;

        public void Update()
        {
            if (EditorManager.Instance != null)
            {
                go.SetActive(EditorManager.Instance.IsObjectEditable());
            }
        }
    }
}
