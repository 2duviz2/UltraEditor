using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class LevelInfoObject : SavableObject
    {
        public string tipOfTheDay = "Hi!";
        public string levelLayer = "ULTRAEDITOR /// CUSTOM LEVEL";
        public string levelName = "%SAVE%";
        public bool playMusicOnDoorOpen = true;
        public bool changeLighting = false;
        public Vector3 ambientColor = new Vector3(255, 255, 255);
        public float intensityMultiplier = 1f;

        public static LevelInfoObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            LevelInfoObject levelInfoObject = target.AddComponent<LevelInfoObject>();
            if (spawnedObject != null) spawnedObject.levelInfoObject = levelInfoObject;
            return levelInfoObject;
        }

        public void createLevelInfo()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            if (changeLighting)
                updateLight();
        }

        void updateLight()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(ambientColor.x / 255f, ambientColor.y / 255f, ambientColor.z / 255f);
            RenderSettings.reflectionIntensity = intensityMultiplier;
        }

        public void update()
        {
            if (changeLighting)
                updateLight();
        }
    }
}