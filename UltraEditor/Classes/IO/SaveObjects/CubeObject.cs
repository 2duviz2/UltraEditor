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
        public bool _isTrigger = false;
        public bool isTrigger
        { get
            {
                return _isTrigger;
            }
            set
            {
                _isTrigger = value;
                if (GetComponent<Collider>() != null)
                    GetComponent<Collider>().isTrigger = _isTrigger;
            }
        }

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
            if (GetComponent<Collider>() != null)
                _isTrigger = GetComponent<Collider>().isTrigger;
            if (matType != _matType || matTiling != _matTiling)
            {
                if (matType != _matType)
                    EditorManager.Instance.SetAlert("Material changed!", "Info!", new Color(0.25f, 1f, 0.25f));
                _matType = matType;
                if (matTiling != _matTiling)
                    EditorManager.Instance.SetAlert("Material edits related to tiling only update when reloading the editor.", "Info!");
                _matTiling = matTiling;
                GetComponent<MaterialChoser>()?.ProcessMaterial(matType, matTiling);
            }
        }
    }
}