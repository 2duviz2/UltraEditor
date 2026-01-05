/*
 * commented out since idk how to handle all the different types of savable objects
 * 
 * i gave up 5 seconds after cloning the loading file
 * 
 * im so good at programming
 * 
using System;
using System.Collections.Generic;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using static UnityEngine.Object;

namespace UltraEditor.Classes.IO;

/// <summary> Handles loading levels </summary>
public class Saving
{
    /// <summary> List of actions to be ran when saving a level depending on the type of object it is. </summary>
    public static Dictionary<Type, Action<SavableObjectData, GameObject>> listeners;

    /// <summary> Loads all the listeners for different savable object types. </summary>
    public static void Load()
    {
        /*
        
        AddListener("TEMPLATE", (data, workingObject) =>
        {

        });

        *//*

        AddListener("CubeObject", (data, workingObject) => 
            CubeObject.Create(workingObject, (MaterialChoser.materialTypes)data.Data[0]));

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

            workingObject.GetComponent<ArenaObject>().onlyWave = (bool)data.Data[0];
            workingObject.GetComponent<ArenaObject>().enemyIds = (List<string>)data.Data[1];
        });

        AddListener("NextArenaObject", (data, workingObject) =>
        {
            NextArenaObject.Create(workingObject);

            workingObject.GetComponent<NextArenaObject>().lastWave = (bool)data.Data[0];
            workingObject.GetComponent<NextArenaObject>().enemyCount = (int)data.Data[1];
            workingObject.GetComponent<NextArenaObject>().enemyIds = (List<string>)data.Data[3];
            workingObject.GetComponent<NextArenaObject>().toActivateIds = (List<string>)data.Data[4];
        });

        AddListener("ActivateObject", (data, workingObject) =>
        {
            ActivateObject.Create(workingObject);

            workingObject.GetComponent<ActivateObject>().toActivateIds = (List<string>)data.Data[0];
            workingObject.GetComponent<ActivateObject>().toDeactivateIds = (List<string>)data.Data[1];
            workingObject.GetComponent<ActivateObject>().canBeReactivated = (bool)data.Data[2];
        });

        AddListener("CheckpointObject", (data, workingObject) =>
        {
            CheckpointObject.Create(workingObject);

            workingObject.GetComponent<CheckpointObject>().rooms = (List<string>)data.Data[0];
            workingObject.GetComponent<CheckpointObject>().roomsToInherit = (List<string>)data.Data[1];
        });

        AddListener("DeathZone", (data, workingObject) =>
        {
            DeathZoneObject.Create(workingObject);

            workingObject.GetComponent<DeathZoneObject>().notInstaKill = (bool)data.Data[0];
            workingObject.GetComponent<DeathZoneObject>().damage = (int)data.Data[1];
            workingObject.GetComponent<DeathZoneObject>().affected = (AffectedSubjects)data.Data[2];
        });

        AddListener("Light", (data, workingObject) =>
        {
            LightObject.Create(workingObject);

            workingObject.GetComponent<LightObject>().intensity = (int)data.Data[0];
            workingObject.GetComponent<LightObject>().range = (int)data.Data[1];
            workingObject.GetComponent<LightObject>().type = (LightType)data.Data[2];
        });
    }

    /// <summary> Adds a listener for that type of savable object. </summary>
    /// <param name="type"></param>
    /// <param name="listener"></param>
    public static void AddListener(string type, Action<SavableObjectData, GameObject> listener) =>
        listeners.Add(type, listener);
}
*/