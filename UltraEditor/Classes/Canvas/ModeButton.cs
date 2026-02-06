namespace UltraEditor.Classes.Canvas;

using System.Collections.Generic;
using UnityEngine;

internal class ModeButton : MonoBehaviour
{
    public static List<ModeButton> buttons = new List<ModeButton>();
    public CameraSelector.SelectionMode mode;
    public GameObject selectedObject;

    void Start()
    {
        buttons.Add(this);
    }

    void OnDestroy()
    {
        buttons.Remove(this);
    }

    public void Click()
    {
        if (EditorManager.Instance != null && EditorManager.Instance.cameraSelector != null)
        {
            EditorManager.Instance.cameraSelector.selectionMode = mode;
        }
    }

    public static void UpdateButtons()
    {
        foreach (var item in buttons)
        {
            if (item.mode != EditorManager.Instance.cameraSelector.selectionMode)
            {
                item.selectedObject.SetActive(false);
            }
            else
            {
                item.selectedObject.SetActive(true);
            }
        }
    }
}
