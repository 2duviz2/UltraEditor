namespace UltraEditor.Classes.Canvas;

using UnityEngine;
using UnityEngine.UI;

internal class ResetScrollbar : MonoBehaviour
{
    public float value = 1;

    void Start()
    {
        ResetPos();
    }

    public void ResetPos()
    {
        GetComponent<Scrollbar>().value = value;
    }
}
