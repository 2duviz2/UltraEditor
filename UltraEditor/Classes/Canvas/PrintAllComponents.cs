namespace UltraEditor.Classes.Canvas;

using System.Linq;
using TMPro;
using UltraEditor.Classes.Editor;
using UnityEngine;

public class PrintAllComponents : MonoBehaviour
{
    public TMP_Text text;
    public string prefix;

    public void Start()
    {
        text.text = prefix + string.Join(
        "<br><br>",
        EditorComponentsList.GetMonoBehaviourTypes(true).Select(t => t.Name)
    );

    }
}
