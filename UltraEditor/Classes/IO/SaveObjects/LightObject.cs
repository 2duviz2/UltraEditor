using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class LightObject : SavableObject
    {
        public float intensity = 1f;
        public float range = 10f;
        public Vector3 color = Vector3.one * 255;
        public LightType type = LightType.Point;

        public static LightObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            LightObject lightObject = target.AddComponent<LightObject>();
            if (spawnedObject != null) spawnedObject.lightObject = lightObject;
            return lightObject;
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
            light.color = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
        }
    }
}