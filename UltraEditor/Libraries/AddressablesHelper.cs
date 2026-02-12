namespace UltraEditor.Libraries;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

/// <summary> Helper class for addressables. </summary>
public static class AddressablesHelper
{
    /// <summary> IResourceLocator for the AddressablesMainContentCatalog. </summary>
    public static IResourceLocator MainAddressablesLocator =>
        Addressables.ResourceLocators.FirstOrDefault(loc => loc.LocatorId == "AddressablesMainContentCatalog");

    /// <summary> Grabs a list of all the addressable keys from the game. </summary>
    public static IEnumerable<object> GetAddressableKeys() =>
        MainAddressablesLocator?.Keys ?? [];

    /// <summary> Grabs a list of every prefabs addressable key, filtering out for dupes. </summary>
    public static List<string> GetPrefabAddressableKeys()
    {
        List<string> keys = [];
        foreach (object key in GetAddressableKeys())
            if (MainAddressablesLocator.Locate(key, typeof(GameObject), out _/*We dont need the IResourceLocation*/) && !keys.Contains(key))
                keys.Add(key.ToString());

        return keys;
    }

    /// <summary> Cache list of all used addressable assets. </summary>
    public static Dictionary<string, object> CachedAddressableAssets = [];

    /// <summary> Synchronously loads an asset via addressables with its addressable key. </summary>
    public static T Ass<T>(string key)
    {
        if (CachedAddressableAssets.TryGetValue(key + typeof(T).Name, out object cachedAsset))
            return (T)cachedAsset;

        T asset = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
        if (asset != null)
            CachedAddressableAssets.Add(key + typeof(T).Name, asset);
        else
            Plugin.LogError("Failed to load asset: " + key);

        return asset;
    }
}
