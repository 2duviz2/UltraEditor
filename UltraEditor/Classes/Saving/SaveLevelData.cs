namespace UltraEditor.Classes.Saving;

using System;
using System.Collections.Generic;
using UnityEngine;

public class UnpackedLevel
{
    /// <summary> Level name, for UI and SceneHelper.CurrentScene in the future when we stop relying on cybergrind. </summary>
    public string name;

    /// <summary> Level description, for UI and extra stuff level creators would ever like to add to their levels. </summary>
    public string description;

    /// <summary> Name of the creator of the level, this doesn't need to be consistent between all their levels but its recommended. (For UI aswell) </summary>
    public string creator;

    /// <summary> Version of the editor that the level was made in. </summary>
    public Version editorVersion;

    /// <summary> Time of which the level was saved/built at. </summary>
    public DateTime buildDate;

    /// <summary> Unique ID for the level to identify it out of a group. </summary>
    public string uniqueIdentifier;

    /// <summary> The thumbnail for the level. </summary>
    public Sprite thumbnail;

    /// <summary> The music played in the level. </summary>
    public AudioClip music;

    /// <summary> All the data of all saved objects. </summary>
    public List<SavableObjectData> savedObjects; // these aren't unpacked til during load
}

public class SaveLevelData
{
    /// <summary> Level name, for UI and SceneHelper.CurrentScene in the future when we stop relying on cybergrind. </summary>
    public string name;

    /// <summary> Level description, for UI and extra stuff level creators would ever like to add to their levels. </summary>
    public string description;

    /// <summary> Name of the creator of the level, this doesn't need to be consistent between all their levels but its recommended. (For UI aswell) </summary>
    public string creator;

    /// <summary> Version of the editor that the level was made in. </summary>
    public string editorVersion;

    /// <summary> Time of which the level was saved/built at. </summary>
    public DateTime buildDate;

    /// <summary> Unique ID for the level to identify it out of a group. </summary>
    public string uniqueIdentifier;

    /// <summary> How many bytes the thumbnail takes up in the .uterus file. </summary>
    public int thumbnailSize;

    /// <summary> How many bytes the music takes up in the .uterus file. </summary>
    public int musicSize;

    /// <summary> The name of the music file. </summary>
    public string musicName;

    /// <summary> All the data of all saved objects. </summary>
    public List<SavableObjectData> savedObjects;
}

public class SavableObjectData
{
    /// <summary> GameObject Identifier. </summary>
    public string Id;
    /// <summary> Name of the GameObject. </summary>
    public string Name;
    /// <summary> Layer that the GameObject is on. </summary>
    public int Layer;
    /// <summary> The GameObject's tag. </summary>
    public string Tag;
    /// <summary> The Position data of the GameObject. </summary>
    public SerializableVector3 Position;
    /// <summary> The Rotation data of the GameObject. </summary>
    public SerializableVector3 EulerAngles;
    /// <summary> The Scale data of the GameObject. </summary>
    public SerializableVector3 Scale;
    /// <summary> The current activity of the GameObject. </summary>
    public bool Active;
    /// <summary> The Parent of the GameObject. </summary>
    public string Parent;
    /// <summary> The type of saveable object this is. </summary>
    public string Type;
    /// <summary> Extra data from like CubeObject for example. </summary>
    public List<object> Data;
}

[Serializable]
public struct SerializableVector3(Vector3 v)
{
    public float x = v.x;
    public float y = v.y;
    public float z = v.z;

    public readonly Vector3 ToVector3() => new(x, y, z);

    public static implicit operator Vector3(SerializableVector3 sv) => sv.ToVector3();
    public static implicit operator SerializableVector3(Vector3 v) => new(v);
}


public class SaveData
{
    public string typeFull;
    public List<FieldData> data;

    public class FieldData
    {
        public string Field;
        public string FieldType;
        public object Data;
    }
}

public class CubeObjectData : SavableObjectData {
    public int matType;
}

public class PrefabObjectData : SavableObjectData {
    public string PrefabAsset;
}

public class ArenaObjectData : SavableObjectData {
    public List<string> enemyIds;
    public bool onlyWave;
}

public class NextArenaObjectData : SavableObjectData
{
    public List<string> enemyIds;
    public List<string> toActivateIds;
    public bool lastWave;
    public int enemyCount;
}

public class ActivateObjectData : SavableObjectData
{
    public List<string> toActivateIds;
    public List<string> toDeactivateIds;
    public bool canBeReactivated;
}

public class CheckpointObjectData : SavableObjectData
{
    public List<string> rooms;
    public List<string> roomsToInherit;
}