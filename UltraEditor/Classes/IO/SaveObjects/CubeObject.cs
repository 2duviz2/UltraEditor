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
        public MaterialChoser.shapes shape;
        MaterialChoser.materialTypes _matType;
        float _matTiling = 0.25f;
        MaterialChoser.shapes _shape;
        public bool _fixMaterialTiling = false;
        public bool fixMaterialTiling;
        public bool _isTrigger = false;
        public bool isTrigger
        {
            get
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

        public override void Tick()
        {
            if (GetComponent<Collider>() != null)
                _isTrigger = GetComponent<Collider>().isTrigger;
            if (matType != _matType || matTiling != _matTiling || shape != _shape || fixMaterialTiling != _fixMaterialTiling)
            {
                if (matType != _matType)
                    EditorManager.Instance.SetAlert("Material changed!", "Info!", new Color(0.25f, 1f, 0.25f));
                if (shape != _shape)
                    EditorManager.Instance.SetAlert("Shape changed!", "Info!", new Color(0.25f, 1f, 0.25f));
                if (matTiling != _matTiling)
                    EditorManager.Instance.SetAlert("Tiling changed!", "Info!");
                if (fixMaterialTiling != _fixMaterialTiling)
                    EditorManager.Instance.SetAlert("Tiling fix changed!", "Info!");
                _matType = matType;
                _shape = shape;
                _matTiling = matTiling;
                _fixMaterialTiling = fixMaterialTiling;
                GetComponent<MaterialChoser>()?.ProcessMaterial(matType, matTiling, shape, fixMaterialTiling);
            }

            if (Time.timeScale > 0)
            {
                if (isTrigger)
                {
                    if (GetComponent<Collider>() != null)
                        Destroy(GetComponent<Collider>());
                }
            }
        }
    }
}