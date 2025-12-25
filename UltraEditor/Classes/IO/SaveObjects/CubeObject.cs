using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class CubeObject : SavableObject
    {
        public MaterialChoser.materialTypes matType;
        MaterialChoser.materialTypes _matType;

        public static CubeObject Create(GameObject target, MaterialChoser.materialTypes materialType)
        {
            CubeObject obj = target.AddComponent<CubeObject>();
            obj.matType = materialType;
            MaterialChoser.Create(target, materialType);
            return obj;
        }

        public void Tick()
        {
            if (matType != _matType)
            {
                _matType = matType;
                GetComponent<MaterialChoser>()?.ProcessMaterial(matType);
            }
        }
    }
}