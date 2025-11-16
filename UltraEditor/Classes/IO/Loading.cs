using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using static UnityEngine.Object;

namespace UltraEditor.Classes.IO;

/// <summary> Handles loading levels </summary>
public class Loading
{
    /// <summary> List of actions to be ran when loading a level depending on the type of object it is. </summary>
    public static Dictionary<string, Action<SavableObjectData, GameObject>> listeners = [];

    /// <summary> Loads all the listeners for different savable object types. </summary>
    public static void Load()
    {
        /*
        
        AddListener("TEMPLATE", (data, workingObject) =>
        {

        });

        */

        AddListener("CubeObject", (data, workingObject) => 
            CubeObject.Create(workingObject, Enum<MaterialChoser.materialTypes>(data.Data[0])));

        AddListener("PrefabObject", (data, workingObject) => 
        {
            GameObject newObj = EditorManager.Instance.SpawnAsset((string)data.Data[0], true);
            newObj.transform.position = workingObject.transform.position;
            newObj.transform.eulerAngles = workingObject.transform.eulerAngles;
            newObj.transform.localScale = workingObject.transform.localScale;
            newObj.layer = workingObject.layer;
            newObj.tag = workingObject.tag;
            newObj.SetActive(workingObject.activeSelf);
            newObj.AddComponent<SpawnedObject>();
            newObj.GetComponent<SpawnedObject>().ID = workingObject.GetComponent<SpawnedObject>().ID;
            newObj.GetComponent<SpawnedObject>().parentID = workingObject.GetComponent<SpawnedObject>().parentID;
            Destroy(workingObject);
        });

        AddListener("ArenaObject", (data, workingObject) =>
        {
            ArenaObject.Create(workingObject);
            Plugin.LogInfo($"ArenaObject:: onlyWave: {(bool)data.Data[0]} | enemyIds: {string.Join(", ", StringList(data.Data[1]))}");

            workingObject.GetComponent<ArenaObject>().onlyWave = (bool)data.Data[0];
            workingObject.GetComponent<ArenaObject>().enemyIds = StringList(data.Data[1]);
        });

        AddListener("NextArenaObject", (data, workingObject) =>
        {
            NextArenaObject nextArenaObject = NextArenaObject.Create(workingObject);
            Plugin.LogInfo($"NextArenaObject:: lastWave: {(bool)data.Data[0]} | enemyCount: {Convert.ToInt32(data.Data[1])} | enemyIds: {string.Join(", ", StringList(data.Data[2]))} | toActivateIds: {string.Join(", ", StringList(data.Data[3]))}");

            nextArenaObject.lastWave = (bool)data.Data[0];
            nextArenaObject.enemyCount = Convert.ToInt32(data.Data[1]);
            nextArenaObject.enemyIds = StringList(data.Data[2]);
            nextArenaObject.toActivateIds = StringList(data.Data[3]);
        });

        AddListener("ActivateObject", (data, workingObject) =>
        {
            ActivateObject.Create(workingObject);

            workingObject.GetComponent<ActivateObject>().toActivateIds = StringList(data.Data[0]);
            workingObject.GetComponent<ActivateObject>().toDeactivateIds = StringList(data.Data[1]);
            workingObject.GetComponent<ActivateObject>().canBeReactivated = (bool)data.Data[2];
        });

        AddListener("CheckpointObject", (data, workingObject) =>
        {
            CheckpointObject.Create(workingObject);

            workingObject.GetComponent<CheckpointObject>().rooms = StringList(data.Data[0]);
            workingObject.GetComponent<CheckpointObject>().roomsToInherit = StringList(data.Data[1]);
        });

        AddListener("DeathZone", (data, workingObject) =>
        {
            DeathZoneObject.Create(workingObject);

            workingObject.GetComponent<DeathZoneObject>().notInstaKill = (bool)data.Data[0];
            workingObject.GetComponent<DeathZoneObject>().damage = (int)data.Data[1];
            workingObject.GetComponent<DeathZoneObject>().affected = Enum<AffectedSubjects>(data.Data[2]);
        });

        AddListener("Light", (data, workingObject) =>
        {
            LightObject.Create(workingObject);

            workingObject.GetComponent<LightObject>().intensity = (float)data.Data[0];
            workingObject.GetComponent<LightObject>().range = (float)data.Data[1];
            workingObject.GetComponent<LightObject>().type = Enum<LightType>(data.Data[2]);
        });
    }

    /// <summary> Invokes a listener for that type of savable object. </summary>
    public static void InvokeListener(SavableObjectData data, GameObject workingObject)
    {
        if (listeners.Count == 0) Load();

        if (listeners.TryGetValue(data.Type, out var listener))
            try { listener(data, workingObject); }
            catch (Exception ex) { 
                Plugin.LogError($"EXCEPTION WHILE INVOKING LOAD LISTENER({data.Type}):\n{ex}\n{ex.Message}", ex.StackTrace); 
            }
    }

    /// <summary> Adds a listener for that type of savable object. </summary>
    public static void AddListener(string type, Action<SavableObjectData, GameObject> listener) =>
        listeners.Add(type, listener);

    #region tools

    //private static Dictionary<object, Type> enumCache = [];

    private static T Enum<T>(object obj) => 
        (T)System.Enum.Parse(typeof(T), obj.ToString());
    
    /*private static T Enumerator<T>(object obj) where T : struct
    {
        //if (enumCache.TryGetValue(obj, out var cacheResult))
        //    return Enum.GetValues(cacheResult).Cast<T>()
        if (Enum.TryParse(obj.ToString(), out T result))
            return result;

        throw new ArgumentException($"Loading enum failed... 3:");
    }*/


    /*private static T Obj<T>(object obj)
    {
        if (obj is JToken token)
            return token.ToObject<T>();

        if (obj is T t)
            return t;

        return (T)Convert.ChangeType(obj, typeof(T));
    }

    private static bool Boolean(object obj) =>
        Obj<bool>(obj);

    private static int Int(object obj) =>
        Obj<int>(obj);*/

    private static List<string> StringList(object obj) =>
        (obj as JArray)?.ToObject<List<string>>();

    #endregion
}