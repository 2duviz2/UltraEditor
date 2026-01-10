using System;
using UltraEditor.Libraries;

namespace UltraEditor.Classes.Editor
{
    public class EditorComponent
    {
        public string description;
        public Type componentType;
        public bool isTrigger;

        public EditorComponent(Type componentType, bool isTrigger, string description = "Description not set for this component")
        {
            this.description = TextPatcher.Patch($"<b>{componentType.Name}</b><br>{description}");
            this.componentType = componentType;
            this.isTrigger = isTrigger;
            EditorComponentsList.editorComponents.Add(this);
        }
    }
}