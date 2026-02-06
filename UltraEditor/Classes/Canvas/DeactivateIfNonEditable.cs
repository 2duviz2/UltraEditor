namespace UltraEditor.Classes.Canvas;

using UnityEngine;

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
