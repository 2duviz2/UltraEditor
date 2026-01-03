using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class CubeObject : SavableObject
    {
        public MaterialChoser.materialTypes matType;
        public float matTiling = 0.25f;
        MaterialChoser.materialTypes _matType;
        float _matTiling = 0.25f;

        public static CubeObject Create(GameObject target, MaterialChoser.materialTypes materialType)
        {
            CubeObject obj = target.AddComponent<CubeObject>();
            obj.matType = materialType;
            obj._matType = materialType;
            MaterialChoser.Create(target, materialType);
            return obj;
        }

        public void Tick()
        {
            if (matType != _matType || matTiling != _matTiling)
            {
                _matType = matType;
                _matTiling = matTiling;
                GetComponent<MaterialChoser>()?.ProcessMaterial(matType, matTiling);
                EditorManager.Instance.SetAlert("Material edits related to tiling only update when reloading the editor.", "Info!");
            }
        }
    }
}