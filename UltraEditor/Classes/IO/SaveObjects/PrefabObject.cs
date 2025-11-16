using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
