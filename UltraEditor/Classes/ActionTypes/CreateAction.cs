namespace UltraEditor.Classes.ActionTypes;

using UltraEditor.Classes.ActionTypes.Base;
using UnityEngine;

/// <summary> Action for when we create an obj uwu meow </summary>
public class CreateAction(GameObject CreatedObject) : IEditorAction
{
    /// <summary> The type of this action. </summary>
    public ActionType Type => ActionType.Create;

    /// <summary> Destroys the object and remembers it for for <see cref="Redo"/>. </summary>
    public void Undo()
    {
        Stored = Object.Instantiate(CreatedObject);
        Stored.SetActive(false);
        Stored.name = "HIDEINHIERARCHY";

        // store obj attributes for Redo();
        StoredActive = CreatedObject.activeSelf;
        StoredParent = CreatedObject.transform.parent; 
        StoredName = CreatedObject.name;

        Object.Destroy(CreatedObject);
    }

    /// <summary> A stored copy of the GameObject we deleted in for <see cref="Undo"/> </summary>
    public GameObject Stored = null;

    /// <summary> Keep track of the active self of the created obj before it was deleted. </summary>
    public bool StoredActive;

    /// <summary> Keep track of the parent of the created obj before it was deleted. </summary>
    public Transform StoredParent;

    /// <summary> Keep track of the name of the created obj before it was deleted. </summary>
    public string StoredName;

    /// <summary> Recreate the object and destroy the stored object. </summary>
    public void Redo()
    {
        CreatedObject = Object.Instantiate(Stored, StoredParent);
        CreatedObject.SetActive(StoredActive);
        CreatedObject.name = StoredName;

        Object.Destroy(Stored);
        EditorManager.Instance.SelectObject(CreatedObject);
    }

    /// <summary> When destroyed, delete the stored GameObject since we won't need it anymore. </summary>
    public void Dispose()
    {
        if (Stored != null) 
            Object.Destroy(Stored);
    }
}
