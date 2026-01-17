using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class PrefabObject : SavableObject
    {
        public string PrefabAsset;

        public static PrefabObject Create(GameObject target, string path)
        {
            PrefabObject obj = target.AddComponent<PrefabObject>();
            obj.PrefabAsset = path;
            return obj;
        }

        public override void Create()
        {
            if (transform.lossyScale.magnitude > 10)
            {
                NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
                mod.ignoreFromBuild = true;
            }
        }
    }
}
