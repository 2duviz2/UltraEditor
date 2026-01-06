using System;
using System.Collections.Generic;
using System.Text;

namespace UltraEditor.Classes.Editor
{
    public class EditorComponent
    {
        public string componentName = "Undefined component name";
        public Type componentType;
        public bool isTrigger;

        public EditorComponent(Type componentType, bool isTrigger, string componentName = "Undefined")
        {
            if (componentName != "Undefined")
                this.componentName = componentName;
            else
                this.componentName = componentType.ToString();
            this.componentType = componentType;
            this.isTrigger = isTrigger;
            EditorComponentsList.editorComponents.Add(this);
        }
    }
}
