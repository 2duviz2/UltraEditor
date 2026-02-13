namespace UltraEditor.Classes;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
        CurrentFolder = "Assets/"; // start in the assets folder
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
    }
}