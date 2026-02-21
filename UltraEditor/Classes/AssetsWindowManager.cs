namespace UltraEditor.Classes;

using GameConsole.Commands;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UltraEditor.Classes.Canvas;
using UltraEditor.Libraries;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Manages/Handles everything for the assets window. </summary>
public class AssetsWindowManager : MonoBehaviour
{
    public static AssetsWindowManager Instance;

    /// <summary> Serves to put the warning about internal assets when changing folder for the first time </summary>
    public static bool HasChangedFolder;

    /// <summary> Camera used to generate the previews inside the UltraEditor folder </summary>
    public Camera PreviewCamera;

    /// <summary> Preview render texture for the camera </summary>
    public RenderTexture PreviewTexture;

    /// <summary> Preview light :3 </summary>
    public Light PreviewLight;

    /// <summary> Hidden AssetItem template. </summary>
    public AssetItem Template;

    /// <summary> Hidden AssetFolder template. </summary>
    public AssetFolder FolderTemplate;

    /// <summary> The text that says the path to the currently open folder in the assets window. </summary>
    public TextMeshProUGUI AssetsFolderPathText;

    /// <summary> The currently open folder. </summary>
    [HideInInspector]
    public string CurrentFolder;

    /// <summary> Assign the Instance. </summary>
    public void Awake() =>
        Instance = this;

    /// <summary> Setup the window. </summary>
    public void Start()
    {
        CurrentFolder = "Assets/UltraEditor/"; // start in the assets folder
        GetComponentInParent<Animator>().speed *= 2f; // faster :3
        CreatePreviewCamera();
        Refresh();
    }

    /// <summary> Goes back to the previous folder in the path. </summary>
    public void PreviousFolder()
    {
        int end = CurrentFolder[..^1].LastIndexOf('/') + 1;
        if (end == 0) return;

        CurrentFolder = CurrentFolder[..end];
        Refresh();

        if (!HasChangedFolder) WarnAboutFolders();
    }

    /// <summary> Creates a preview of the camera idk lmao </summary>
    public void CreatePreviewCamera()
    {
        PreviewTexture = new RenderTexture(100, 100, 16);

        GameObject cam = new GameObject("Preview camera");
        PreviewCamera = cam.AddComponent<Camera>();
        PreviewCamera.transform.position = new Vector3(0, -10000, 0);
        PreviewCamera.cullingMask = LayerMask.GetMask("Invisible");
        PreviewCamera.forceIntoRenderTexture = true;
        PreviewCamera.targetTexture = PreviewTexture;
        PreviewCamera.enabled = false;
        PreviewCamera.clearFlags = CameraClearFlags.SolidColor;
        PreviewCamera.backgroundColor = new Color(0, 0, 0, 0);
        PreviewLight = PreviewCamera.gameObject.AddComponent<Light>();
        PreviewLight.type = LightType.Spot;
        PreviewLight.range = 250;
        PreviewLight.spotAngle = 120;
        PreviewLight.cullingMask = LayerMask.GetMask("Invisible");
    }

    /// <summary> Refreshes the items in the assets window. </summary>
    public void Refresh()
    {
        if (!Folders.TryGetValue(CurrentFolder, out List<string> keys))
            return;

        // delete children
        for (int i = 2; i < transform.childCount; i++) 
            Destroy(transform.GetChild(i).gameObject);

        // load folders in this current folder
        foreach (string folder in Folders.Keys.Where(key => key.StartsWith(CurrentFolder) && key[CurrentFolder.Length..].Occurrences('/') == 1))
        {
            AssetFolder newAssetFolder = Instantiate(FolderTemplate, transform);
            newAssetFolder.folderPath = folder;
            newAssetFolder.folderName = folder[(folder[..^1].LastIndexOf('/')+1)..^1]; // this just gets the name of the folder it looks scary ik its cuz im bad at programming

            newAssetFolder.gameObject.SetActive(true);
        }

        // load assets in this current folder
        foreach (string key in keys)
        {
            AssetItem newAssetItem = Instantiate(Template, transform);
            newAssetItem.assetName = Path.GetFileNameWithoutExtension(key);
            newAssetItem.assetPath = key;

            // get item color based on the key-folder its in
            string keyFolder = Path.GetDirectoryName(key).Replace('\\', '/') + '/'; // key folders is the folder, based off of the key :3
            if (ItemColorListeners.TryGetValue(keyFolder, out Color col) || ItemColorListeners.TryGetValue(key, out col))
                newAssetItem.GetComponent<Image>().color = col;

            newAssetItem.gameObject.SetActive(true);

            if (CurrentFolder == "Assets/UltraEditor/" && EditorManager.canOpenEditor)
                SetPreview(newAssetItem.assetItemPreview, key);
            else
                newAssetItem.assetItemPreview.gameObject.SetActive(false);
        }

        AssetsFolderPathText.text = CurrentFolder;

        StartCoroutine(transform.parent.Find("Scrollbar").GetComponent<ResetScrollbar>().Reset());
    }

    public void WarnAboutFolders()
    {
        HasChangedFolder = true;
        EditorManager.Instance.SetAlert("External assets can break your game and force you to restart if you don't know what you're doing, please experiment with pacience.", "Warning!");
    }

    #region Loading

    /// <summary> A dictionary of folder paths and a list of all the key's of the assets inside that folder. </summary>
    public static Dictionary<string, List<string>> Folders = [];

    /// <summary> Dictionary of listeners to decide the color of a item asset in the assets window based on the key-folder the asset is in. </summary>
    public static Dictionary<string, Color> ItemColorListeners = [];

    /// <summary> Loads everything important ahead of time meow rawr miaow :333 </summary>
    public static void Load()
    {
        // folders
        LoadFolders();
        LoadDefaultAssets();

        // we want items in specific key-folders to be colored differently cuz it looks cool, so load those listeners too :3
        RegisterItemColorListener(new(1f, 1f, 0f), "Assets/Prefabs/Levels/Special Rooms/", "Assets/Prefabs/Levels/", "Bonus");
        RegisterItemColorListener(new(0f, 0.9f, 1f), "Assets/Prefabs/Levels/Interactive/", "AltarBlueOff", "AltarRedOff");
        RegisterItemColorListener(new(0.25f, 1f, 0.75f), "Assets/Prefabs/Levels/Obstacles/");
        RegisterItemColorListener(new(0.5f, 0.4f, 1f), "Assets/Prefabs/Levels/Decorations/");
        RegisterItemColorListener(new(0.4f, 0f, 0.4f), "Assets/Prefabs/Levels/Doors/");
        RegisterItemColorListener(new(1f, 0.2f, 0.2f), "Assets/Prefabs/Enemies/");
    }

    /// <summary> Registers a new pair of ItemColorListeners. </summary>
    public static void RegisterItemColorListener(Color col, params List<string> keyFolders) =>
        keyFolders.ForEach(kF => ItemColorListeners.Add(kF, col));

    /// <summary> Loads all the folders and assets in that folder into the Folders dictionary. </summary>
    public static void LoadFolders()
    {
        List<string> keys = AssHelper.GetPrefabAddressableKeys();

        foreach (string key in keys)
        {
            string folder = Path.GetDirectoryName(key).Replace('\\', '/') + "/";
            if (!Folders.ContainsKey(folder))
            {
                Folders.Add(folder, GetAssetsInFolder(folder));

                foreach (int index in folder[..^1].Occurences('/'))
                {
                    string subFolder = folder[..(index + 1)];

                    if (!Folders.ContainsKey(subFolder))
                        Folders.TryAdd(subFolder, GetAssetsInFolder(subFolder));
                }
            }
        }

        List<string> GetAssetsInFolder(string folder) =>
            [.. keys.Where(key => key.StartsWith(folder) && !key[folder.Length..].Contains('/'))
                .Concat(folder == "Assets/" ? GetAssetsInFolder("") : [])]; // this is just for the assets folder since theres assets with no folder :P
    }

    /// <summary> Oh god please help </summary>
    public static void LoadDefaultAssets() =>
        Folders.Add("Assets/UltraEditor/", [
            "Assets/Prefabs/Enemies/Zombie.prefab",
            "Assets/Prefabs/Enemies/Projectile Zombie.prefab",
            "Assets/Prefabs/Enemies/Super Projectile Zombie.prefab",
            "Assets/Prefabs/Enemies/ShotgunHusk.prefab",
            "Assets/Prefabs/Enemies/MinosBoss.prefab",
            "Assets/Prefabs/Enemies/Stalker.prefab",
            "Assets/Prefabs/Enemies/Sisyphus.prefab",
            "Assets/Prefabs/Enemies/Ferryman.prefab",
            "Assets/Prefabs/Enemies/SwordsMachineNonboss.prefab",
            "Assets/Prefabs/Enemies/Drone.prefab",
            "Assets/Prefabs/Enemies/Streetcleaner.prefab",
            "Assets/Prefabs/Enemies/V2.prefab",
            "Assets/Prefabs/Enemies/Mindflayer.prefab",
            "Assets/Prefabs/Enemies/V2 Green Arm Variant.prefab",
            "Assets/Prefabs/Enemies/Turret.prefab",
            "Assets/Prefabs/Enemies/Gutterman.prefab",
            "Assets/Prefabs/Enemies/Guttertank.prefab",
            "Assets/Prefabs/Enemies/Spider.prefab",
            "Assets/Prefabs/Enemies/Cerberus.prefab",
            "Assets/Prefabs/Enemies/CerberusStatue.prefab",
            "Assets/Prefabs/Enemies/Mass.prefab",
            "Assets/Prefabs/Enemies/Idol.prefab",
            "Assets/Prefabs/Enemies/Mannequin.prefab",
            "Assets/Prefabs/Enemies/MannequinPoserWithEnemy.prefab",
            "Assets/Prefabs/Enemies/Minotaur.prefab",
            "Assets/Prefabs/Enemies/Gabriel.prefab",
            "Assets/Prefabs/Enemies/Virtue.prefab",
            "Assets/Prefabs/Enemies/Gabriel 2nd Variant.prefab",
            "Assets/Prefabs/Enemies/Cancerous Rodent.prefab",
            "Assets/Prefabs/Enemies/Very Cancerous Rodent.prefab",
            "Assets/Prefabs/Enemies/Mandalore.prefab",
            //"Assets/Prefabs/Enemies/Wicked.prefab", duviz why server no work
            "Assets/Prefabs/Enemies/Big Johninator.prefab",
            "Assets/Prefabs/Enemies/Flesh Prison.prefab",
            "Assets/Prefabs/Enemies/Flesh Prison 2.prefab",
            "Assets/Prefabs/Enemies/MinosPrime.prefab",
            "Assets/Prefabs/Enemies/SisyphusPrime.prefab",
            "Assets/Prefabs/Enemies/Puppet.prefab",
            "Assets/Prefabs/Enemies/SwordsMachine Agony.prefab",
            "Assets/Prefabs/Enemies/SwordsMachine Tundra.prefab",
            "Assets/Prefabs/Enemies/CentaurRocketLauncher.prefab",
            "Assets/Prefabs/Enemies/Brain.prefab",
            "Assets/Prefabs/Enemies/CentaurTower.prefab",
            "Assets/Prefabs/Enemies/CentaurMortar.prefab",
            "Assets/Prefabs/Levels/Decorations/SuicideTreeHungry.prefab",
            "Assets/Prefabs/Levels/Interactive/Altar (Blue).prefab",
            "AltarBlueOff",
            "Assets/Prefabs/Levels/Interactive/Altar (Red).prefab",
            "AltarRedOff",
            "Assets/Prefabs/Levels/Interactive/Altar (Torch) Variant.prefab",
            "Assets/Prefabs/Levels/Interactive/Altar.prefab",
            "Assets/Prefabs/Levels/Interactive/Breakable.prefab",
            "Assets/Prefabs/Levels/Interactive/Crate.prefab",
            "Assets/Prefabs/Levels/Interactive/GrapplePoint.prefab",
            "Assets/Prefabs/Levels/Interactive/GrapplePointSlingshot Variant.prefab",
            "Assets/Prefabs/Levels/Interactive/JumpPad.prefab",
            "Assets/Prefabs/Levels/Interactive/Plank.prefab",
            "Assets/Prefabs/Levels/Interactive/GlassFloor.prefab",
            "Assets/Prefabs/Levels/Interactive/GlassWall.prefab",
            "Assets/Prefabs/Levels/Interactive/ChandelierFerry.prefab",
            "Assets/Prefabs/Fishing/Fish Pickup Template.prefab",
            "DuvizPlushFixed",
            "Assets/Prefabs/Items/Florp Throwable.prefab",
            "Assets/Prefabs/Items/KITR.prefab",
            "Assets/Prefabs/Levels/Interactive/ElectricityBox.prefab",
            "Assets/Prefabs/Sandbox/Procedural Water Brush.prefab",
            "Assets/Prefabs/Sandbox/Lava.prefab",
            "Assets/Prefabs/Sandbox/Procedural Hot Sand.prefab",
            "BlackholeChaos/Blackhole",
            "BlackholeChaos/Whitehole",
            "Assets/Prefabs/Levels/Obstacles/Grinders.prefab",
            "Assets/Prefabs/Levels/Obstacles/EnergyBeamHuge.prefab",
            "Assets/Prefabs/Levels/Obstacles/Fan.prefab",
            "Assets/Prefabs/Levels/Decorations/Metro Chair.prefab",
            "Assets/Prefabs/Levels/Decorations/Metro Chair Double.prefab",
            "Assets/Prefabs/Levels/Decorations/Metro Light.prefab",
            "Assets/Prefabs/Levels/Decorations/Metro Light Ground.prefab",
            "Assets/Prefabs/Levels/Decorations/Pipes.prefab",
            "Assets/Prefabs/Levels/Decorations/PipeMulch.prefab",
            "Assets/Prefabs/Levels/Decorations/Skeleton.prefab",
            "Assets/Prefabs/Levels/Decorations/SkeletonStake.prefab",
            "Assets/Prefabs/Levels/Decorations/Speaker (Noise).prefab",
            "Assets/Prefabs/Levels/Decorations/SpiralPillar.prefab",
            "Assets/Prefabs/Levels/Decorations/Streetlight.prefab",
            "Assets/Prefabs/Levels/Decorations/Bed.prefab",
            "Assets/Prefabs/Levels/Decorations/CeilingLantern.prefab",
            "Assets/Prefabs/Levels/Decorations/CandlePile.prefab",
            "Assets/Prefabs/Levels/Decorations/Arch.prefab",
            "Assets/Prefabs/Levels/Decorations/Chair.prefab",
            "Assets/Prefabs/Levels/Decorations/Furnace.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 1.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 2.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 3.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 1 Off.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 2 Off.prefab",
            "Assets/Prefabs/Levels/Decorations/Tree 3 Off.prefab",
            "Assets/Prefabs/Levels/Decorations/TreeFake 1.prefab",
            "Assets/Prefabs/Levels/Decorations/TreeFake 2.prefab",
            "Assets/Prefabs/Levels/Decorations/TreeFake 1.prefab",
            "Assets/Prefabs/Levels/Decorations/Vent.prefab",
            "Assets/Prefabs/Levels/Decorations/Clock.prefab",
            "Assets/Prefabs/Levels/Decorations/Crane.prefab",
            "Assets/Prefabs/Levels/Decorations/GrossHand.prefab",
            "Assets/Prefabs/Levels/Decorations/HarborLight.prefab",
            "Assets/Prefabs/Levels/Decorations/SuicideTree1.prefab",
            "Assets/Prefabs/Levels/Decorations/SuicideTree2.prefab",
            "Assets/Prefabs/Levels/Decorations/SuicideTree3.prefab",
            "Assets/Prefabs/Levels/Decorations/SuicideTreeHungry.prefab(Active)",
            "Assets/Prefabs/Levels/Decorations/Water Well.prefab",
            "ImCloudingIt",
            "Assets/Models/Enemies/Centaur/CentaurFull/Midpoly/Centaur_MidPoly.fbx",
            "Assets/Prefabs/Levels/Decorations/BigGas.prefab",
            "Assets/Particles/Environment/GasParticleSquare.prefab",
            "Assets/Particles/Environment/GasParticle.prefab",
            "Assets/Particles/Fire.prefab",
            "Assets/Prefabs/Levels/Doors/Barrier.prefab",
            "Assets/Prefabs/Levels/Doors/BunkerDoor.prefab",
            "Assets/Prefabs/Levels/Doors/Door (Large) With Controllers.prefab",
            "Assets/Prefabs/Levels/Doors/DoorGreed.prefab",
            "Assets/Prefabs/Levels/Doors/DoorLust.prefab",
            "Assets/Prefabs/Levels/Doors/DoorMouth.prefab",
            "Assets/Prefabs/Levels/Doors/FerryDoor.prefab",
            "Assets/Prefabs/Levels/Doors/GardenDoor.prefab",
            "Assets/Prefabs/Levels/Doors/GardenDoorBig.prefab",
            "Assets/Prefabs/Levels/Doors/GothicDoor.prefab",
            "Assets/Prefabs/Levels/Doors/Limbo Large Door.prefab",
            "Assets/Prefabs/Levels/Doors/Limbo Large Door Smaller.prefab",
            "Assets/Prefabs/Levels/Doors/Limbo Large Door Smallest.prefab",
            "Assets/Prefabs/Levels/Doors/MetroBlockDoor.prefab",
            "Assets/Prefabs/Levels/Doors/Underwater Tunnel.prefab",
            "Assets/Prefabs/Levels/Doors/ViolenceHallDoor.prefab",
            "Assets/Prefabs/Levels/Doors/Wrath Circular Door.prefab",
            "Assets/Prefabs/Levels/Doors/Hellgate 1.prefab",
            "Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab",
            "Assets/Prefabs/Levels/Checkpoint.prefab",
            "Bonus",
            "Assets/Prefabs/Levels/BonusDualWield Variant.prefab",
            "Assets/Prefabs/Levels/BonusSuperCharge.prefab",
            "Assets/Prefabs/Levels/DualWieldPowerup.prefab",
        ]);

    public void SetPreview(Image preview, string path) => StartCoroutine(WaitForPreview(preview, path));

    public static Dictionary<string, Sprite> cachedPreviews = [];
    IEnumerator WaitForPreview(Image preview, string path)
    {
        if (preview == null) yield break;

        if (cachedPreviews.TryGetValue(path, out Sprite cachedAsset))
            preview.sprite =  cachedAsset;

        GameObject previewObj = EditorManager.Instance.SpawnAsset(path, isLoading: true);

        SetLayerRecursively(previewObj, LayerMask.NameToLayer("Invisible"));

        Bounds b = GetBoundsRecursive(previewObj);
        Vector3 center = b.center;
        float size = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);

        float distance = 5;
        if (size > 5)
            distance = 15;

        previewObj.transform.position = new Vector3(0, -10000, distance);
        if (size > 5)
            previewObj.transform.Rotate(0, 180, 0);

        PreviewCamera.enabled = false;
        PreviewCamera.Render();

        preview.sprite = RenderTextureToSprite(PreviewTexture);

        cachedPreviews[path] = preview.sprite;

        DestroyImmediate(previewObj);
    }

    public Bounds GetBoundsRecursive(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    public static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public static Sprite RenderTextureToSprite(RenderTexture rt)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        RenderTexture.active = prev;

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    #endregion
}