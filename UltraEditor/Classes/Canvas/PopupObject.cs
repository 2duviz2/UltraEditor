namespace UltraEditor.Classes.Canvas;

using TMPro;
using UnityEngine;

public class PopupObject : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text contentText;

    public void SetPopupInfo(string title, string content)
    {
        titleText.text = title;
        contentText.text = content;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
