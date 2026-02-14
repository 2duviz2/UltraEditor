namespace UltraEditor.Classes;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UltraEditor.Classes.Canvas;
using UltraEditor.Libraries;
using UnityEngine;

/// <summary> Manages/Handles everything for the assets window. </summary>
public class AssetsWindowManager : MonoBehaviour
{
    public static AssetsWindowManager Instance;

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
        Refresh();
    }

    /// <summary> Goes back to the previous folder in the path. </summary>
    public void PreviousFolder()
    {
        int end = CurrentFolder[..^1].LastIndexOf('/') + 1;
        if (end == 0) return;

        CurrentFolder = CurrentFolder[..end];
        Refresh();
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

            newAssetItem.gameObject.SetActive(true);
        }

        AssetsFolderPathText.text = CurrentFolder;

        StartCoroutine(transform.parent.Find("Scrollbar").GetComponent<ResetScrollbar>().Reset());
    }

    /// <summary> A dictionary of folder paths and a list of all the key's of the assets inside that folder. </summary>
    public static Dictionary<string, List<string>> Folders = [];

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

        LoadDefaultAssets();
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
            "Assets/Prefabs/Enemies/Wicked.prefab",
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
}