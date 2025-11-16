using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    internal class LightObject : SavableObject
    {
        public float intensity = 1f;
        public float range = 10f;
        public LightType type = LightType.Point;

        public static LightObject Create(GameObject target)
        {
            LightObject obj = target.AddComponent<LightObject>();
            return obj;
        }

        public void createLight()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            Light light = gameObject.AddComponent<Light>();

            light.intensity = intensity;
            light.range = range;
            light.type = type;
        }
    }
}