namespace UltraEditor.Classes;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltraEditor.Libraries;
using UnityEngine;

/// <summary> Manages/Handles everything for the assets window. </summary>
public class AssetsWindowManager : MonoBehaviour
{
    /// <summary> Hidden AssetItem template. </summary>
    public AssetItem Template;

    /// <summary> Hidden AssetFolder template. </summary>
    public AssetFolder FolderTemplate;

    /// <summary> Setup the window. </summary>
    public void Start()
    {
        foreach (string key in AddressablesHelper.GetPrefabAddressableKeys())
        {
            AssetItem newAssetItem = Instantiate(Template, transform);
            newAssetItem.assetName = Path.GetFileNameWithoutExtension(key);
            newAssetItem.assetPath = key;

            newAssetItem.gameObject.SetActive(true);
        }
    }
}