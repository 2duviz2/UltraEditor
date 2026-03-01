namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for whenever we rotate an obj. </summary>
public class RotateAction(GameObject Obj, Vector3 PreRot) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Rotate;

    /// <summary> Sets the rotation of an object back to its previous rotation and remembers its new rotation for <see cref="Redo"/> </summary>
    public void Undo()
    {
        NewRot = Obj.transform.eulerAngles;
        Obj.transform.eulerAngles = PreRot;
    }

    /// <summary> The new rotation of the object. </summary>
    public Vector3 NewRot;

    /// <summary> Sets the rotation of an object to its new rotation. </summary>
    public void Redo() =>
        Obj.transform.eulerAngles = NewRot;

    public void Dispose() { }
}
