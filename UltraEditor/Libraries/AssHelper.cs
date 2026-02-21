namespace UltraEditor.Libraries;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary> Helper class for AddreSSable ASSets. </summary>
public static class AssHelper
{
    #region Keys

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
            if (MainAddressablesLocator.Locate(key, typeof(GameObject), out IList<IResourceLocation> locs) && !keys.Contains(locs[0].PrimaryKey))
                keys.Add(locs[0].PrimaryKey);
        
        keys.Sort();
        return keys;
    }

    #endregion
    #region Asset Loading

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

    public static bool CheckIfLoaded<T>(string key)
    {
        if (CachedAddressableAssets.TryGetValue(key + typeof(T).Name, out object cachedAsset))
            return true;
        return false;
    }

    #endregion

    extension(string str)
    {
        /// <summary> How many times a char occurs in string. </summary>
        public int Occurrences(char lookUp)
        {
            int count = 0;
            foreach (char c in str)
                if (c == lookUp) count++;

            return count;
        }

        /// <summary> The indexes of every occurence of a char in a string. </summary>
        public IEnumerable<int> Occurences(char lookUp)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] == lookUp) yield return i;
        }
    }
}
