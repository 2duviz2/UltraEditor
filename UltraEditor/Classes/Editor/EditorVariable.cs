using System;
using System.Collections.Generic;
using System.Text;

namespace UltraEditor.Classes.Editor
{
    public class EditorVariable
    {
        public string varName;
        public string varDisplay;
        public Type parentComponent;

        public EditorVariable (string varName, string varDisplay, Type parentComponent)
        {
            this.varName = varName;
            this.varDisplay = varDisplay;
            this.parentComponent = parentComponent;

            EditorVariablesList.editorVariables.Add(this);
        }
    }
}
