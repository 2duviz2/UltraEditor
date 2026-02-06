namespace UltraEditor.Classes.Canvas;

using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    public string url;

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
