namespace UltraEditor.Classes.TempScripts;

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DeleteOnEditor : MonoBehaviour
{
    public void Update()
    {
        if (EditorManager.Instance.editorOpen)
            Destroy(gameObject);
    }
}