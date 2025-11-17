using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Object;

namespace UltraEditor.Classes.IO;

/// <summary> Handles loading levels </summary>
public class Loading
{
    /// <summary> List of actions to be ran when loading a level depending on the type of object it is. </summary>
    public static Dictionary<string, Action<SavableObjectData, GameObject, SpawnedObject>> listeners = [];

    /// <summary> Loads all the listeners for different savable object types. </summary>
    public static void Load()
    {
        /*
        
        AddListener("TEMPLATE", (data, workingObject) =>
        {

        });

        */

        AddListener("CubeObject", (data, workingObject, spawnedObject) => 
            CubeObject.Create(workingObject, Enum<MaterialChoser.materialTypes>(data.Data[0])));

        AddListener("PrefabObject", (data, workingObject, spawnedObject) => 
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

        AddListener("ArenaObject", (data, workingObject, spawnedObject) =>
        {
            ArenaObject.Create(workingObject, spawnedObject);
            //Plugin.LogInfo($"ArenaObject:: onlyWave: {(bool)data.Data[0]} | enemyIds: {string.Join(", ", StringList(data.Data[1]))}");

            workingObject.GetComponent<ArenaObject>().onlyWave = (bool)data.Data[0];
            workingObject.GetComponent<ArenaObject>().enemyIds = StringList(data.Data[1]);
        });

        AddListener("NextArenaObject", (data, workingObject, spawnedObject) =>
        {
            NextArenaObject nextArenaObject = NextArenaObject.Create(workingObject, spawnedObject);
            //Plugin.LogInfo($"NextArenaObject:: lastWave: {(bool)data.Data[0]} | enemyCount: {Convert.ToInt32(data.Data[1])} | enemyIds: {string.Join(", ", StringList(data.Data[2]))} | toActivateIds: {string.Join(", ", StringList(data.Data[3]))}");

            nextArenaObject.lastWave = (bool)data.Data[0];
            nextArenaObject.enemyCount = Convert.ToInt32(data.Data[1]);
            nextArenaObject.enemyIds = StringList(data.Data[2]);
            nextArenaObject.toActivateIds = StringList(data.Data[3]);
        });

        AddListener("ActivateObject", (data, workingObject, spawnedObject) =>
        {
            ActivateObject.Create(workingObject, spawnedObject);

            workingObject.GetComponent<ActivateObject>().toActivateIds = StringList(data.Data[0]);
            workingObject.GetComponent<ActivateObject>().toDeactivateIds = StringList(data.Data[1]);
            workingObject.GetComponent<ActivateObject>().canBeReactivated = (bool)data.Data[2];
        });

        AddListener("CheckpointObject", (data, workingObject, spawnedObject) =>
        {
            CheckpointObject.Create(workingObject, spawnedObject);

            workingObject.GetComponent<CheckpointObject>().rooms = StringList(data.Data[0]);
            workingObject.GetComponent<CheckpointObject>().roomsToInherit = StringList(data.Data[1]);
        });

        AddListener("DeathZone", (data, workingObject, spawnedObject) =>
        {
            DeathZoneObject.Create(workingObject, spawnedObject);

            workingObject.GetComponent<DeathZoneObject>().notInstaKill = (bool)data.Data[0];
            workingObject.GetComponent<DeathZoneObject>().damage = (int)data.Data[1];
            workingObject.GetComponent<DeathZoneObject>().affected = Enum<AffectedSubjects>(data.Data[2]);
        });

        AddListener("Light", (data, workingObject, spawnedObject) =>
        {
            LightObject.Create(workingObject, spawnedObject);

            workingObject.GetComponent<LightObject>().intensity = (float)data.Data[0];
            workingObject.GetComponent<LightObject>().range = (float)data.Data[1];
            workingObject.GetComponent<LightObject>().type = Enum<LightType>(data.Data[2]);
        });

        AddListener("MusicObject", (data, workingObject, spawnedObject) =>
        {
            MusicObject.Create(workingObject, spawnedObject);

            bool calmOnline = (bool)data.Data[0];
            string calmName = (string)data.Data[1];

            workingObject.GetComponent<MusicObject>().calmThemeOnline = calmOnline;
            workingObject.GetComponent<MusicObject>().calmThemePath = calmName;

            if (!calmOnline)
            {
                byte[] music = (byte[])data.Data[2];

                string tempFile = Path.Combine(Application.temporaryCachePath, calmName);
                File.WriteAllBytes(tempFile, music);

                workingObject.GetComponent<MusicObject>().calmThemePath = tempFile;
            }

            bool battleOnline = calmOnline ? (bool)data.Data[2] : (bool)data.Data[3];
            string battleName = calmOnline ? (string)data.Data[3] : (string)data.Data[4];

            workingObject.GetComponent<MusicObject>().battleThemeOnline = battleOnline;
            workingObject.GetComponent<MusicObject>().battleThemePath = battleName;

            if (!battleOnline)
            {
                byte[] music = (byte[])data.Data[5];

                string tempFile = Path.Combine(Application.temporaryCachePath, battleName);
                File.WriteAllBytes(tempFile, music);

                workingObject.GetComponent<MusicObject>().battleThemePath = tempFile;
            }
        });
    }

    /// <summary> Invokes a listener for that type of savable object. </summary>
    public static void InvokeListener(SavableObjectData data, GameObject workingObject, SpawnedObject spawnedObject)
    {
        if (listeners.Count == 0) Load();

        if (listeners.TryGetValue(data.Type, out var listener))
            try { listener(data, workingObject, spawnedObject); }
            catch (Exception ex) { 
                Plugin.LogError($"EXCEPTION WHILE INVOKING LOAD LISTENER({data.Type}):\n{ex}\n{ex.Message}", ex.StackTrace); 
            }
    }

    #region tools

    /// <summary> Adds a listener for that type of savable object. </summary>
    public static void AddListener(string type, Action<SavableObjectData, GameObject, SpawnedObject> listener) =>
        listeners.Add(type, listener);

    private static T Enum<T>(object obj) => 
        (T)System.Enum.Parse(typeof(T), obj.ToString());

    private static List<string> StringList(object obj) =>
        (obj as JArray)?.ToObject<List<string>>();

    #endregion
}