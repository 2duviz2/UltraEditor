namespace UltraEditor.Classes.IO;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Object;

public class IO
{
    public static async void testUnpack(string path, bool load)
    {
        var u = await UnpackLevel(path, load);
        Plugin.LogInfo($"\nname: {u.name}\ndesc: {u.description}\ncreator: {u.creator}\nversion: {u.editorVersion}\nbuilddate: {u.buildDate}\nid: {u.uniqueIdentifier}\nsavedObjects: {string.Join(", ", from blob in u.savedObjects where blob != null select blob.Name)}");

        if (load)
            AudioSource.PlayClipAtPoint(u.music, NewMovement.Instance.transform.position);
        else
        {
            var obj = GameObject.Find("Image :3") ?? new GameObject("Image :3");
            obj.transform.parent = GameObject.Find("Canvas").transform;
            obj.transform.localPosition = new(0f, 0f, 0f);
            (obj.GetComponent<Image>() ?? obj.AddComponent<Image>()).sprite = u.thumbnail;
        }
    }

    public static SavableObjectData CreateAndPopulate(SavableObject obj, string Type = null) => new()
    {
        Id = EditorManager.GetIdOfObj(obj.gameObject),
        Name = obj.Name,
        Layer = obj.gameObject.layer,
        Tag = obj.gameObject.tag,
        Position = obj.Position,
        EulerAngles = obj.EulerAngles,
        Scale = obj.Scale,
        Active = obj.gameObject.activeSelf,
        Parent = obj.transform.parent != null ? EditorManager.GetIdOfObj(obj.transform.parent.gameObject) : "",
        Type = Type,
        Data = []
    };

    public static void Save(string name, string description, string creator, string path, string thumbnailPath = null, string musicPath = null, string GUID = null)
    {
        // UltraEditor.Classes.IO.IO.Save("new saving system test uwu :3", "testing the new saving system rn", "Bryan_-000-", "newsavetesting", "C:\\Users\\freda\\Downloads\\absolute cinema.jpg", "C:\\Users\\freda\\Music\\fe\\femtanyl - LOVESICK, CANNIBAL! (feat takihasdied).mp3", "Bryan_-000-.ULTRAEDITOR.SaveSystemTest");
        // UltraEditor.Classes.IO.IO.Load("newsavetesting.uterus");
        // UltraEditor.Classes.IO.IO.testUnpack("newsavetesting.uterus", false);

        List<byte> imageBytes = thumbnailPath != null && File.Exists(thumbnailPath) ? [.. File.ReadAllBytes(thumbnailPath)] : [];
        List<byte> musicBytes = musicPath != null && File.Exists(musicPath) ? [.. File.ReadAllBytes(musicPath)] : [];
        SaveLevelData level = new()
        {
            name = name,
            description = description,
            creator = creator,
            editorVersion = new(Plugin.Version),
            buildDate = DateTime.UtcNow,
            uniqueIdentifier = GUID ?? Guid.NewGuid().ToString("N"),
            thumbnailSize = imageBytes.Count,
            musicSize = musicBytes.Count,
            musicName = musicPath == null ? null : Path.GetFileName(musicPath),
            savedObjects = []
        };

        foreach (var obj in FindObjectsOfType<CubeObject>(true))
        {
            if (obj.GetComponent<ActivateArena>() != null && obj.GetComponent<Collider>().isTrigger)
            {
                GameObject ob = obj.gameObject;
                Destroy(obj.GetComponent<CubeObject>());
                ArenaObject o = ArenaObject.Create(ob);
                continue;
            }

            if (obj.GetComponent<ActivateNextWave>() != null)
            {
                GameObject ob = obj.gameObject;
                Destroy(obj.GetComponent<CubeObject>());
                NextArenaObject o = NextArenaObject.Create(ob);
                continue;
            }

            if (obj.GetComponent<Light>() != null || obj.GetComponent<ActivateObject>() != null || obj.GetComponent<CheckpointObject>() != null || obj.GetComponent<DeathZone>() != null)
            {
                continue;
            }

            SavableObjectData cubeObj = CreateAndPopulate(obj, "CubeObject");
            cubeObj.Data.Add(obj.matType);
            level.savedObjects.Add(cubeObj);
            Plugin.LogInfo($"added cubeObj to saved: {cubeObj.Name} | savedObjects: null? {(level.savedObjects == null ? "yes" : "no")}, {string.Join(", ", from sav in level.savedObjects where sav != null select sav.Name)}");
        }

        foreach (var obj in FindObjectsOfType<PrefabObject>(true))
        {
            if (obj.GetComponent<CheckPoint>() != null) continue;

            SavableObjectData prefabObj = CreateAndPopulate(obj, "PrefabObject");
            prefabObj.Data.Add(obj.PrefabAsset);
            level.savedObjects.Add(prefabObj);
            Plugin.LogInfo($"added prefabObj to saved: {prefabObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<ArenaObject>(true))
        {
            obj.enemyIds.Clear();
            foreach (var e in obj.GetComponent<ActivateArena>().enemies)
            {
                obj.addId(EditorManager.GetIdOfObj(e));
            }

            SavableObjectData arenaObj = CreateAndPopulate(obj, "ArenaObject");
            arenaObj.Data.Add(obj.GetComponent<ActivateArena>().onlyWave);
            arenaObj.Data.Add(obj.enemyIds);
            level.savedObjects.Add(arenaObj);
            Plugin.LogInfo($"added arenaObj to saved: {arenaObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<NextArenaObject>(true))
        {
            obj.enemyIds.Clear();
            obj.toActivateIds.Clear();
            if (obj.GetComponent<ActivateNextWave>().nextEnemies != null)
                foreach (var e in obj.GetComponent<ActivateNextWave>().nextEnemies)
                {
                    obj.addEnemyId(EditorManager.GetIdOfObj(e));
                }
            if (obj.GetComponent<ActivateNextWave>().toActivate != null)
                foreach (var e in obj.GetComponent<ActivateNextWave>().toActivate)
                {
                    obj.addToActivateId(EditorManager.GetIdOfObj(e));
                }

            SavableObjectData nextObj = CreateAndPopulate(obj, "NextArenaObject");
            nextObj.Data.Add(obj.GetComponent<ActivateNextWave>().lastWave);
            nextObj.Data.Add(obj.GetComponent<ActivateNextWave>().enemyCount);
            nextObj.Data.Add(obj.enemyIds);
            nextObj.Data.Add(obj.toActivateIds);
            level.savedObjects.Add(nextObj);
            Plugin.LogInfo($"added nextObj to saved: {nextObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<ActivateObject>(true))
        {
            obj.toActivateIds.Clear();
            obj.toDeactivateIds.Clear();
            foreach (var e in obj.toActivate)
            {
                obj.addToActivateId(EditorManager.GetIdOfObj(e));
            }
            foreach (var e in obj.toDeactivate)
            {
                obj.addtoDeactivateId(EditorManager.GetIdOfObj(e));
            }

            SavableObjectData actObj = CreateAndPopulate(obj, "ActivateObject");
            actObj.Data.Add(obj.toActivateIds);
            actObj.Data.Add(obj.toDeactivateIds);
            actObj.Data.Add(obj.canBeReactivated);
            level.savedObjects.Add(actObj);
            Plugin.LogInfo($"added actObj to saved: {actObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<CheckPoint>(true))
        {
            while (obj.GetComponent<CheckpointObject>() != null)
                Destroy(obj.GetComponent<CheckpointObject>());
            CheckpointObject co = CheckpointObject.Create(obj.gameObject);

            foreach (var e in obj.rooms)
            {
                if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                    co.addRoomId(EditorManager.GetIdOfObj(e, new Vector3(-10000, 0, 0)));
                else
                    co.addRoomId(EditorManager.GetIdOfObj(e));
            }
            foreach (var e in obj.roomsToInherit)
            {
                co.addRoomToInheritId(EditorManager.GetIdOfObj(e));
            }

            SavableObjectData coObj = CreateAndPopulate(co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null
                ? co.transform.parent.GetComponent<CheckpointObject>()
                : co, "CheckpointObject");
            coObj.Data.Add(co.rooms);
            coObj.Data.Add(co.roomsToInherit);
            level.savedObjects.Add(coObj);
            Plugin.LogInfo($"added coObj to saved: {coObj.Name}");

            Destroy(obj.GetComponent<CheckpointObject>());
        }

        foreach (var obj in FindObjectsOfType<CheckpointObject>(true))
        {
            if (obj.transform.childCount != 0) continue;

            SavableObjectData actObj = CreateAndPopulate(obj, "CheckpointObject");
            actObj.Data.Add(obj.rooms);
            actObj.Data.Add(obj.roomsToInherit);
            level.savedObjects.Add(actObj);
            Plugin.LogInfo($"added actObj to saved: {actObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<DeathZone>(true))
        {
            if (obj.GetComponent<CubeObject>() == null) continue;

            SavableObjectData deathObj = CreateAndPopulate(obj.gameObject.AddComponent<SavableObject>(), "DeathZone");
            deathObj.Data.Add(obj.notInstakill);
            deathObj.Data.Add(obj.damage);
            deathObj.Data.Add(obj.affected);
            level.savedObjects.Add(deathObj);
            Plugin.LogInfo($"added deathObj to saved: {deathObj.Name}");
        }

        foreach (var obj in FindObjectsOfType<Light>(true))
        {
            if (obj.GetComponent<CubeObject>() == null) continue;

            SavableObjectData lightObj = CreateAndPopulate(obj.gameObject.AddComponent<SavableObject>(), "Light");
            lightObj.Data.Add(obj.intensity);
            lightObj.Data.Add(obj.range);
            lightObj.Data.Add(obj.type);
            level.savedObjects.Add(lightObj);
            Plugin.LogInfo($"added lightObj to saved: {lightObj.Name}");
        }

        //level.savedObjects.Add(new() { Name = "DEFAULT TESTING SAVE OBJ" });
        Plugin.LogInfo($"savedObjects: {string.Join(", ", from sav in level.savedObjects where sav != null select sav.Name)}");

        // write the file
        var fileStream = new FileStream(Path.Combine(Application.persistentDataPath, $"{path}.uterus"), FileMode.Create, FileAccess.Write);
        BinaryWriter binaryWriter = new(fileStream, Encoding.UTF8, leaveOpen: false);

        // add level stuff
        binaryWriter.Write(JsonConvert.SerializeObject(level));

        // add thumbnail
        binaryWriter.Write([.. imageBytes]);

        // add music
        binaryWriter.Write([.. musicBytes]);

        // also btw if u dont close it then u'll get like "this file is open in ULTRAKILL" msgs when u try to open the .uterus
        // so i might add a try catch to everything before so just incase something fails, it closes the stream and writer
        binaryWriter.Close();
        fileStream.Close();
    }

    public static async void Load(string sceneName)
    {
        string path = Application.persistentDataPath + $"/{sceneName}";

        if (!File.Exists(path))
        {
            Plugin.LogError("Save not found!");
            return;
        }

        UnpackedLevel unpack = await UnpackLevel(sceneName, true);

        foreach (SavableObjectData saveData in unpack.savedObjects)
        {
            // create the gameobject to add stuff too and make it as a spawned object
            GameObject workingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workingObject.AddComponent<SpawnedObject>();

            // add generic stuff from CreateAndPopulate as its in every object
            workingObject.GetComponent<SpawnedObject>().ID = saveData.Id;
            workingObject.name = saveData.Name;
            workingObject.layer = saveData.Layer;
            workingObject.tag = saveData.Tag;
            workingObject.transform.position = saveData.Position;
            workingObject.transform.eulerAngles = saveData.EulerAngles;
            workingObject.transform.localScale = saveData.Scale;
            workingObject.gameObject.SetActive(saveData.Active);
            workingObject.GetComponent<SpawnedObject>().parentID = saveData.Parent;

            // find a listener for this exact type of object and invoke it
            Loading.InvokeListener(saveData, workingObject);
        }

        // unpack.music; heres the song for the level if it has custom music, custom music doesnt exist yet which is why this is commented

        foreach (var obj in FindObjectsOfType<SpawnedObject>(true))
        {
            if (obj.parentID != "")
            {
                foreach (var findingObj in FindObjectsOfType<SpawnedObject>(true))
                {
                    if (obj.parentID == findingObj.ID)
                    {
                        obj.transform.SetParent(findingObj.transform, true);
                        break;
                    }
                }
            }
        }

        foreach (var obj in FindObjectsOfType<SpawnedObject>(true))
        {
            obj.GetComponent<ArenaObject>()?.createArena();
            obj.GetComponent<NextArenaObject>()?.createArena();
            obj.GetComponent<ActivateObject>()?.createActivator();
            obj.GetComponent<CheckpointObject>()?.createCheckpoint();
            obj.GetComponent<DeathZoneObject>()?.createDeathzone();
            obj.GetComponent<LightObject>()?.createLight();
        }

        if (EditorManager.MissionNameText != null)
        {
            Destroy(EditorManager.MissionNameText.GetComponent<LevelNameFinder>());
            EditorManager.MissionNameText.text = sceneName.Replace(".uterus", "");
        }
    }

    public static async Task<UnpackedLevel> UnpackLevel(string levelName, bool load = false)
    {
        string path = Application.persistentDataPath + $"/{levelName}";
        if (!path.EndsWith(".uterus") || !File.Exists(path))
        {
            Plugin.LogError((path.EndsWith(".uterus")
                ? "Error while loading level data, file doesn't end with .uterus <|> path: "
                : "Error while loading level data, file doesn't exist. <|> path: ")
                + path);
            return null;
        }

        FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new(fileStream);
        SaveLevelData levelData = JsonConvert.DeserializeObject<SaveLevelData>(binaryReader.ReadString());
        Plugin.LogInfo($"Loading level, name: {levelData.name} <|> path: {path}");

        UnpackedLevel unpacked = new()
        {
            name = levelData.name,
            description = levelData.description,
            creator = levelData.creator,
            editorVersion = new(levelData.editorVersion),
            buildDate = levelData.buildDate,
            uniqueIdentifier = levelData.uniqueIdentifier,
            thumbnail = new Sprite(), // doing new Sprite() incase load is false but the level doesnt have a thumbnail
            music = new AudioClip(), // same reason as the new Sprite()
            savedObjects = []
        };

        if (load)
        {
            // we dont need the thumbnail if we're just loading the level, so just skip its bytes
            binaryReader.BaseStream.Seek(levelData.thumbnailSize, SeekOrigin.Current);

            if (levelData.musicSize > 0)
            {
                byte[] music = binaryReader.ReadBytes(levelData.musicSize);

                // temporarly write the music to a file so we can turn it into an audio clip (im sowwy idk any other way to turn audio bytes into an audioclip 3:)
                string tempFile = Path.Combine(Application.temporaryCachePath, levelData.musicName);
                File.WriteAllBytes(tempFile, music);

                AudioType audType = levelData.musicName.ToLower() switch
                {
                    "wav" => AudioType.WAV,
                    "ogg" => AudioType.OGGVORBIS,
                    "mp3" => AudioType.MPEG,
                    "mp4" => AudioType.MPEG,
                    _ => AudioType.UNKNOWN
                };
                var request = UnityWebRequestMultimedia.GetAudioClip("file:///" + tempFile, levelData.musicName.ToLower() switch { "wav" => AudioType.WAV, "ogg" => AudioType.OGGVORBIS, "mp3" => AudioType.MPEG, "mp4" => AudioType.MPEG, _ => AudioType.UNKNOWN });
                var op = request.SendWebRequest();

                var tcs = new TaskCompletionSource<AsyncOperation>();
                op.completed += _ => tcs.SetResult(op);
                await tcs.Task; // turning it into a task since im too lazy to turn this whole thing into a coroutine

                if (request.result != UnityWebRequest.Result.Success)
                    Plugin.LogError($"Could not load level music, request error: {request.error}");
                else
                    unpacked.music = DownloadHandlerAudioClip.GetContent(request);

                File.Delete(tempFile);
            }

            unpacked.savedObjects = levelData.savedObjects;
        }
        else if (levelData.thumbnailSize > 0)
        {
            byte[] imageBytes = binaryReader.ReadBytes(levelData.thumbnailSize);

            Texture2D thumbImage = new(0, 0);
            if (thumbImage.LoadImage(imageBytes))
            {
                thumbImage.Apply();

                unpacked.thumbnail = Sprite.Create(thumbImage, new(0, 0, thumbImage.width, thumbImage.height), new(0.5f, 0.5f));
            }
            else Plugin.LogError("Failed to load the thumbnail of the level into a texture.");
        }

        binaryReader.Close();
        fileStream.Close();

        return unpacked;
    }
}