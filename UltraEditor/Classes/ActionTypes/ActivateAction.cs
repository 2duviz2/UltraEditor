namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for when we enable/disable an obj. </summary>
public class ActivateAction(GameObject obj, bool active) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Activate;

    public void Undo() =>
        obj.SetActive(!active);

    public void Redo() => 
        obj.SetActive(active);

    public void Dispose() { }
}
