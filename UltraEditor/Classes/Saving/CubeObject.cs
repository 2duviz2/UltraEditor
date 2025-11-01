using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    public class CubeObject : SavableObject
    {
        private MaterialChoser.materialTypes _matType;

        public MaterialChoser.materialTypes matType
        {
            get => _matType;
            set
            {
                _matType = value;
                var chooser = GetComponent<MaterialChoser>();
                if (chooser != null)
                    chooser.ProcessMaterial(_matType);
            }
        }

        public static CubeObject Create(GameObject target, MaterialChoser.materialTypes materialType)
        {
            CubeObject obj = target.AddComponent<CubeObject>();
            obj.matType = materialType;
            MaterialChoser.Create(target, materialType);
            return obj;
        }
    }
}