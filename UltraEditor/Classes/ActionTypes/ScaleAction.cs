namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for whenever we scale an obj. </summary>
public class ScaleAction(GameObject Obj, Vector3 PreScale) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Scale;

    /// <summary>  Sets the scale of an object back to its previous scale and remembers its new scale for <see cref="Redo"/> </summary>
    public void Undo()
    {
        NewScale = Obj.transform.localScale;
        Obj.transform.localScale = PreScale;
    }

    /// <summary> The new scale of the object. </summary>
    public Vector3 NewScale;

    /// <summary> Sets the scale of an object to its new scale. </summary>
    public void Redo() =>
        Obj.transform.localScale = NewScale;

    public void Dispose() { }
}
