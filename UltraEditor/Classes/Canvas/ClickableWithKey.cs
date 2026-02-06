namespace UltraEditor.Classes.Canvas;

using UnityEngine;
using UnityEngine.UI;

public class ClickableWithKey : MonoBehaviour
{
    public Button button;

    public void Update()
    {
        if (Input.GetMouseButtonDown(3)) // back button
            button.onClick.Invoke();
    }
}