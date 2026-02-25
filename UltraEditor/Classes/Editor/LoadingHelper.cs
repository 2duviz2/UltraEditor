namespace UltraEditor.Classes.Editor;

using System.Collections.Generic;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;

public static class LoadingHelper
{
    /// <summary>
    /// Returns a list of every found object in the list of ids
    /// </summary>
    /// <param name="ids">A list of ids to search for</param>
    public static List<GameObject> GetObjectsWithIdsList(List<string> ids)
    {
        return [.. GetObjectsWithIds(ids)];
    }

    /// <summary> Cached ids, reset when loading a new scene </summary>
    public static Dictionary<GameObject, string> cachedIds = [];
    public static Dictionary<string, GameObject> reverseCachedIds = [];

    public static void RebuiltCacheForIDs()
    {
        foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
        {
            GetIdOfObj(obj.gameObject, seeking : true);
            continue;
        }
        foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
        {
            GetIdOfObj(obj.gameObject, seeking: true);
            continue;
        }
    }

    /// <summary>
    /// Returns an array of every found object in the list of ids
    /// </summary>
    /// <param name="ids">A list of ids to search for</param>
    public static GameObject[] GetObjectsWithIds(List<string> ids)
    {
        List<GameObject> foundObjects = [];
        foreach (var e in ids)
        {
            reverseCachedIds.TryGetValue(e, out var cached);
            if (cached != null)
            {
                foundObjects.Add(cached);
                continue;
            }
            foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
            {
                string id = GetIdOfObj(obj.gameObject);
                if (e == id)
                {
                    foundObjects.Add(obj.gameObject);
                    break;
                }
            }
        }

        return [.. foundObjects];
    }

    public static uint idIndex = 0;

    /// <summary> Returns an ID for the object </summary>
    /// <param name="obj"> GameObject to get the ID from </param>
    /// <param name="offset"> In case you want to add offset to the position like when using checkpoints </param>
    public static string GetIdOfObj(GameObject obj, Vector3? offset = null, bool seeking = false)
    {
        if (!obj) return string.Empty;

        idIndex++;

        if (cachedIds.TryGetValue(obj, out var cached))
            return cached;

        string idText = string.Empty;
        var spawned = obj.GetComponent<SpawnedObject>();
        if (spawned && spawned.ID != "")
        {
            cachedIds[obj] = spawned.ID;
            return spawned.ID;
        }

        var savable = obj.GetComponent<SavableObject>();

        if (savable != null)
        {
            idText = "(" + idIndex + ")";
        }

        var t = obj.transform;
        Vector3 pos = offset.HasValue ? t.position + offset.Value : t.position;
        string result = $"{idText}{obj.name}{pos}{t.eulerAngles}{t.lossyScale}";

        cachedIds[obj] = result;
        reverseCachedIds[result] = obj;
        return result;
    }
}