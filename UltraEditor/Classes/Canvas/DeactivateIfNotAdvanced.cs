namespace UltraEditor.Classes.Canvas;

using UnityEngine;

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