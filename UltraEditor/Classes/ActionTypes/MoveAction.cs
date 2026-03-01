namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for whenever we move an obj. </summary>
public class MoveAction(GameObject Obj, Vector3 PrePos) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Move;

    /// <summary> Moves an object back to its previous position and remembers its new position for <see cref="Redo"/> </summary>
    public void Undo()
    {
        NewPos = Obj.transform.position;
        Obj.transform.position = PrePos;
    }

    /// <summary> The new position of the object. </summary>
    public Vector3 NewPos;

    /// <summary> Moves an object to its new position. </summary>
    public void Redo() =>
        Obj.transform.position = NewPos;

    public void Dispose() { }
}
