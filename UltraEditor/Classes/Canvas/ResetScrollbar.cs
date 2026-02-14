namespace UltraEditor.Classes.Canvas;

using System.Collections;
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

    public IEnumerator Reset()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        ResetPos();
    }
}
