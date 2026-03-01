namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for when we delete an obj :3 </summary>
public class DeleteAction(GameObject Stored, bool StoredActive, Transform StoredParent, string StoredName) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Delete;

    /// <summary> A stored copy of the GameObject we deleted in for <see cref="Redo"/> </summary>
    public GameObject Obj = null;

    /// <summary> Recreate the object and destroy the stored object. </summary>
    public void Undo()
    {
        Obj = Object.Instantiate(Stored, StoredParent);
        Obj.SetActive(StoredActive);
        Obj.name = StoredName;

        Object.Destroy(Stored);
        EditorManager.Instance.SelectObject(Obj);
    }

    /// <summary> Destroys the object and remembers it for <see cref="Undo"/>. </summary>
    public void Redo()
    {
        Stored = Object.Instantiate(Obj);
        Stored.SetActive(false);
        Stored.name = "HIDEINHIERARCHY";

        // store obj attributes for Undo();
        StoredActive = Obj.activeSelf;
        StoredParent = Obj.transform.parent;
        StoredName = Obj.name;

        Object.Destroy(Obj);
    }

    /// <summary> When destroyed, delete the stored GameObject since we won't need it anymore. </summary>
    public void Dispose()
    {
        if (Stored != null)
            Object.Destroy(Stored);
    }
}
