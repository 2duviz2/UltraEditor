namespace UltraEditor.Classes.ActionTypes;

/// <summary> a bunch of coolio extensions for using IEditorActions </summary>
public static class IEditorActionExtensions
{
    extension(IEditorAction action)
    {
        /// <summary> Invokes <see cref="IEditorAction.Undo"/> and does the lil silly &quot;Undo Done!&quot; pop up :3 </summary>
        public void UndoPop()
        {
            action.Undo();
            EditorManager.Instance.SetAlert("Undo done!", "Info!", new(1f, 0.5f, 0.25f));
        }

        /// <summary> Invokes <see cref="IEditorAction.Redo"/> and does the &quot;Redo Done!&quot; pop up. </summary>
        public void RedoPop()
        {
            action.Redo();
            EditorManager.Instance.SetAlert("Redo done!", "Info!", new(1f, 0.25f, 0.5f));
        }
    }
}
