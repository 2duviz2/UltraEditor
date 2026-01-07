using System;

namespace UltraEditor.Classes.Editor
{
    public class EditorComponent
    {
        public string description = "Undefined component name";
        public Type componentType;
        public bool isTrigger;

        public EditorComponent(Type componentType, bool isTrigger, string description = "Description not set for this component")
        {
            this.description = description;
            this.componentType = componentType;
            this.isTrigger = isTrigger;
            EditorComponentsList.editorComponents.Add(this);
        }
    }
}