namespace UltraEditor.Classes.ActionTypes;

using System;

/// <summary> Abstract class for interacting with any sort of action done in the editor. </summary>
public interface IEditorAction : IDisposable // IDisposable to delete any objs we keep track of when we delete the redo stack
{
    /// <summary> The type of action this is. (this isnt really used anymore but just if u want :3) </summary>
    public ActionType Type { get; }

    /// <summary> Method to be implemented to undo the action. </summary>
    public void Undo();

    /// <summary> Method to be implemented to redo the action. </summary>
    public void Redo();
}

/// <summary> All types of editor actions. mrrp miaow rawr </summary>
public enum ActionType
{
    Create,
    Activate,
    Delete,
    Move,
    Scale,
    Rotate
}