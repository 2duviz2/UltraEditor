namespace UltraEditor.Classes.Editor;

using System.Collections.Generic;
using System.Linq;
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
    public static List<(string, GameObject)> cachedIds = [];

    public static void RebuiltCacheForIDs()
    {
        foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
        {
            string id = GetIdOfObj(obj.gameObject);
            cachedIds.Add((id, obj.gameObject));
            continue;
        }
        foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
        {
            string id = GetIdOfObj(obj.gameObject);
            cachedIds.Add((id, obj.gameObject));
            continue;
        }
    }

    /// <summary>
    /// Returns an array of every found object in the list of ids
    /// </summary>
    /// <param name="ids">A list of ids to search for</param>
    public static GameObject[] GetObjectsWithIds(List<string> ids)
    {
        if (cachedIds.Count == 0)
            RebuiltCacheForIDs();

        List<GameObject> foundObjects = [];
        foreach (var e in ids)
        {
            (string, GameObject) cached = cachedIds.FirstOrDefault(x => x.Item1 == e);
            if (cached.Item2 != null)
            {
                foundObjects.Add(cached.Item2);
                continue;
            }
            foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
            {
                string id = GetIdOfObj(obj.gameObject);
                if (e == id)
                {
                    foundObjects.Add(obj.gameObject);
                    cachedIds.Add((id, obj.gameObject));
                    break;
                }
            }
        }

        return [.. foundObjects];
    }

    /// <summary> Returns an ID for the object </summary>
    /// <param name="obj"> GameObject to get the ID from </param>
    /// <param name="offset"> In case you want to add offset to the position like when using checkpoints </param>
    public static string GetIdOfObj(GameObject obj, Vector3? offset = null)
    {
        if (obj == null) return "";
        return obj.name + (offset == null ? obj.transform.position : obj.transform.position + offset).ToString() + obj.transform.eulerAngles.ToString() + obj.transform.lossyScale;
    }
}