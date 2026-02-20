namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using UltraEditor.Classes.World;
using Unity.AI.Navigation;
using UnityEngine;

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
    [EditorVar("Activate on door open")]
    public GameObject[] activateOnDoorOpen = [];
    public List<string> activateOnDoorOpenIds = [];

    public static LevelInfoObject Create(GameObject target)
    {
        LevelInfoObject levelInfoObject = target.AddComponent<LevelInfoObject>();
        return levelInfoObject;
    }

    public void addToActivateId(string id) => activateOnDoorOpenIds.Add(id);

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        if (changeLighting)
            updateLight();
        UpdateSkybox(true);

        activateOnDoorOpen = LoadingHelper.GetObjectsWithIds(activateOnDoorOpenIds);
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

    OnLevelStart onLevelStart = null;

    public override void Tick()
    {
        if (changeLighting)
            updateLight();
        UpdateSkybox();
        if (Time.timeScale != 0)
        {
            if (onLevelStart == null)
                onLevelStart = OnLevelStart.Instance;

            if (onLevelStart != null)
            {
                onLevelStart.onStart = new()
                {
                    toActivateObjects = activateOnDoorOpen,
                    toDisActivateObjects = [],
                    onActivate = new(),
                    onDisActivate = new()
                };
            }
        }
    }
}