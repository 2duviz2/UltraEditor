namespace UltraEditor.Classes;

using TMPro;
using UnityEngine;

internal class VersionText : MonoBehaviour
{
    public TMP_Text text;

    void Start()
    {
        text.text = $"v{Plugin.GetVersion()}";
    }
}