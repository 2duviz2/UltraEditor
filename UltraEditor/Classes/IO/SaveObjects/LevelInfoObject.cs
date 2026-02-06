using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.World;
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
        public SkyboxManager.Skybox skybox = SkyboxManager.Skybox.BlackSky;
        SkyboxManager.Skybox currentSkybox = SkyboxManager.Skybox.BlackSky;
        public string customSkyboxUrl = "";
        public string currentSkyboxUrl = "";

        public static LevelInfoObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            LevelInfoObject levelInfoObject = target.AddComponent<LevelInfoObject>();
            if (spawnedObject != null) spawnedObject.levelInfoObject = levelInfoObject;
            return levelInfoObject;
        }

        public override void Create()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            if (changeLighting)
                updateLight();
            UpdateSkybox(true);
        }

        public void updateLight()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(ambientColor.x / 255f, ambientColor.y / 255f, ambientColor.z / 255f);
            RenderSettings.reflectionIntensity = intensityMultiplier;
        }

        public void UpdateSkybox(bool force = false)
        {
            bool flag = currentSkybox == skybox && currentSkyboxUrl == customSkyboxUrl && !force;
            if (!flag)
            {
                currentSkybox = skybox;
                currentSkyboxUrl = customSkyboxUrl;
                SkyboxManager.SetSkybox(skybox, customSkyboxUrl);
                MonoSingleton<CameraController>.Instance.cam.clearFlags = ((skybox == SkyboxManager.Skybox.BlackSky) ? CameraClearFlags.Color : CameraClearFlags.Skybox);
            }
        }

        public override void Tick()
        {
            if (changeLighting)
                updateLight();
            UpdateSkybox();
        }
    }
}