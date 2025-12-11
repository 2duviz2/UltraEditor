namespace UltraEditor.Classes.IO;

using Newtonsoft.Json;
using plog.Models;
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
    public static void testUnpack(string path)
    {
        var u = UnpackLevel(path);
        Plugin.LogInfo($"\nname: {u.name}\ndesc: {u.description}\ncreator: {u.creator}\nversion: {u.editorVersion}\nbuilddate: {u.buildDate}\nid: {u.uniqueIdentifier}\nsavedObjects: {string.Join(", ", from blob in u.savedObjects where blob != null select blob.Name)}");

        var obj = GameObject.Find("Image :3") ?? new GameObject("Image :3");
        obj.transform.parent = GameObject.Find("Canvas").transform;
        obj.transform.localPosition = new(0f, 0f, 0f);
        (obj.GetComponent<Image>() ?? obj.AddComponent<Image>()).sprite = u.thumbnail;
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

    public static void Save(string name, string description, string creator, string path, string thumbnailPath = null, string calmThemePath = null, string battleThemePath = null, string GUID = null)
    {
        // UltraEditor.Classes.IO.IO.Save("new saving system test uwu :3", "testing the new saving system rn", "Bryan_-000-", "newsavetesting", "C:\\Users\\freda\\Downloads\\absolute cinema.jpg", "C:\\Users\\freda\\Music\\fe\\femtanyl - LOVESICK, CANNIBAL! (feat takihasdied).mp3", "Bryan_-000-.ULTRAEDITOR.SaveSystemTest");
        // UltraEditor.Classes.IO.IO.Load("newsavetesting.uterus");
        // UltraEditor.Classes.IO.IO.testUnpack("newsavetesting.uterus", false);

        List<byte> imageBytes = thumbnailPath != null && File.Exists(thumbnailPath) ? [.. File.ReadAllBytes(thumbnailPath)] : [];
        SaveLevelData level = new()
        {
            name = name,
            description = description,
            creator = creator,
            editorVersion = new(Plugin.Version),
            MajorVersion = 2,
            buildDate = DateTime.UtcNow,
            uniqueIdentifier = GUID ?? Guid.NewGuid().ToString("N"),
            thumbnailSize = imageBytes.Count,
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

        foreach (var obj in FindObjectsOfType<MusicObject>(true))
        {
            if (obj.GetComponent<SavableObject>() == null) continue;

            SavableObjectData musicObj = CreateAndPopulate(obj.gameObject.AddComponent<SavableObject>(), "MusicObject");
            bool calmOnline = obj.calmThemePath.StartsWith("http");
            musicObj.Data.Add(calmOnline);
            musicObj.Data.Add(calmOnline ? obj.calmThemePath : Path.GetFileName(obj.calmThemePath));
            if (!calmOnline) musicObj.Data.Add(File.ReadAllBytes(obj.calmThemePath));

            bool battleOnline = obj.battleThemePath.StartsWith("http");
            musicObj.Data.Add(battleOnline);
            musicObj.Data.Add(battleOnline ? obj.battleThemePath : Path.GetFileName(obj.battleThemePath));
            if (!battleOnline) musicObj.Data.Add(File.ReadAllBytes(obj.battleThemePath));

            level.savedObjects.Add(musicObj);
        }

        Plugin.LogInfo($"savedObjects: {string.Join(", ", from sav in level.savedObjects where sav != null select sav.Name)}");

        // write the file
        var fileStream = new FileStream(Path.Combine(Application.persistentDataPath, $"{path}.uterus"), FileMode.Create, FileAccess.Write);
        BinaryWriter binaryWriter = new(fileStream, Encoding.UTF8, leaveOpen: false);

        // add level stuff
        binaryWriter.Write(JsonConvert.SerializeObject(level));

        // add thumbnail
        binaryWriter.Write([.. imageBytes]);

        // also btw if u dont close it then u'll get like "this file is open in ULTRAKILL" msgs when u try to open the .uterus
        // so i might add a try catch to everything before so just incase something fails, it closes the stream and writer
        binaryWriter.Close();
        fileStream.Close();
    }

    public class MapData
    {
        public string name;

        public string description;

        public long bundleSize;

        public string bundleName;

        public long thumbSize;

        public string uniqueIdentifier;

        public string author;

        public int version;

        public string[] placeholderPrefabs;

        public string catalog;
    }

    public static void MakeVBlood()
    {
        var log = new plog.Logger("UWU");

        byte[] imageBytes = File.ReadAllBytes("C:\\Users\\freda\\Downloads\\pick aparyt\\tundra default\\icon.png");
        log.Info($"imageBytes: {string.Join(' ', imageBytes.Take(25).Select(b => b.ToString("X2")))}");
        byte[] bundleBytes = File.ReadAllBytes("C:\\Users\\freda\\Downloads\\pick aparyt\\tundra default\\bundle_scenes_all.bundle");
        log.Info($"bundleBytes: {string.Join(' ', bundleBytes.Take(25).Select(b => b.ToString("X2")))}");

        MapData mapData = new() 
        {
            name = "Default Map with a Zombie in the middle",
            description = "Default Map with a Zombie in the middle",
            bundleSize = bundleBytes.Length,
            bundleName = "8b1d219d24bbc9e4c83fab024f226517_scenes_all.bundle",
            thumbSize = imageBytes.Length,
            uniqueIdentifier = "8b1d219d24bbc9e4c83fab024f226517",
            author = "PITR",
            version = 3,
            placeholderPrefabs = [ "FirstRoom" ],
            catalog = Encoding.UTF8.GetString(File.ReadAllBytes("C:\\Users\\freda\\Downloads\\pick aparyt\\catalog.json"))
        };
        log.Info($@"name: {mapData.name}
desc: {mapData.description}
bundleSize: {mapData.bundleSize}
bundleName: {mapData.bundleName}
thumbSize: {mapData.thumbSize}
guid: {mapData.uniqueIdentifier}
author: {mapData.author}
version: {mapData.version}
placeholderPrefabs: {string.Join(", ", mapData.placeholderPrefabs)}
catalog: {mapData.catalog.Substring(0, 100)}");
        
        // write the file
        var fileStream = new FileStream("C:\\Users\\freda\\Downloads\\pick aparyt\\tundra default.blood", FileMode.Create, FileAccess.Write);
        BinaryWriter binaryWriter = new(fileStream, Encoding.UTF8, leaveOpen: false);

        // add level stuff
        binaryWriter.Write(JsonConvert.SerializeObject(mapData));

        // add thumbnail
        binaryWriter.Write(imageBytes, 0, imageBytes.Length);

        // add bundle
        binaryWriter.Write(bundleBytes, 0, bundleBytes.Length);

        // also btw if u dont close it then u'll get like "this file is open in ULTRAKILL" msgs when u try to open the .uterus
        // so i might add a try catch to everything before so just incase something fails, it closes the stream and writer
        binaryWriter.Close();
        fileStream.Close();
    }

    public static void Load(string sceneName)
    {
        //UltraEditor.Classes.IO.IO.Load("new checkpointTest.uterus");
        float startTime = Time.realtimeSinceStartup;
        float time = startTime;

        string path = Application.persistentDataPath + $"/{sceneName}";

        if (!File.Exists(path))
        {
            Plugin.LogError("Save not found!");
            return;
        }

        UnpackedLevel unpack = UnpackLevel(sceneName, true);
        Plugin.LogInfo($"Unpack in {Time.realtimeSinceStartup - time} seconds!");

        if (unpack.MajorVersion != 2)
        {
            Plugin.LogError($"LEVEL IS MADE IN A DIFFERENT MAJOR VERSION, TO LOAD THIS LEVEL USE AN OLDER VERSION OF ULTRAEDITOR.\nLEVEL CREATED IN ULTRAEDITOR VERSION: {unpack.editorVersion}\n{unpack.LogData}");
            return;
        }

        Plugin.LogInfo("Invoking load listeners...");
        time = Time.realtimeSinceStartup;

        foreach (SavableObjectData saveData in unpack.savedObjects)
        {
            // create the gameobject to add stuff too and make it as a spawned object
            GameObject workingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // add generic stuff from CreateAndPopulate as its in every object
            workingObject.name = saveData.Name;
            workingObject.layer = saveData.Layer;
            workingObject.tag = saveData.Tag;
            workingObject.transform.position = saveData.Position;
            workingObject.transform.eulerAngles = saveData.EulerAngles;
            workingObject.transform.localScale = saveData.Scale;
            workingObject.gameObject.SetActive(saveData.Active);

            SpawnedObject spawnedObject = workingObject.AddComponent<SpawnedObject>();
            spawnedObject.ID = saveData.Id;
            spawnedObject.parentID = saveData.Parent;

            // find a listener for this exact type of object and invoke it
            Loading.InvokeListener(saveData, workingObject, spawnedObject);
        }

        Plugin.LogInfo($"Invoking load listeners completed in {Time.realtimeSinceStartup - time} seconds!");
        Plugin.LogInfo("Assigning parents...");
        time = Time.realtimeSinceStartup;

        var allObjs = FindObjectsOfType<SpawnedObject>(true);

        var dict = new Dictionary<string, SpawnedObject>();
        foreach (var o in allObjs)
        {
            if (!string.IsNullOrEmpty(o.ID))
                dict[o.ID] = o;
        }

        foreach (var obj in allObjs)
        {
            if (!string.IsNullOrEmpty(obj.parentID) && dict.TryGetValue(obj.parentID, out var parent))
            {
                obj.transform.SetParent(parent.transform, true);
            }
        }

        Plugin.LogInfo($"Assigned parents in {Time.realtimeSinceStartup - time} seconds!");
        Plugin.LogInfo("Creating objects...");
        time = Time.realtimeSinceStartup;

        foreach (var obj in allObjs)
        {
            obj.arenaObject?.createArena();
            obj.nextArenaObject?.createArena();
            obj.activateObject?.createActivator();
            obj.checkpointObject?.createCheckpoint();
            obj.deathZoneObject?.createDeathzone();
            obj.lightObject?.createLight();
            obj.musicObject?.createMusic();
        }

        Plugin.LogInfo($"Object creation completed in {Time.realtimeSinceStartup - time} seconds!");
        Plugin.LogInfo($"Loading done in {Time.realtimeSinceStartup - startTime} seconds!");

        EditorManager.Instance.cameraSelector.selectedObject = null;
    }

    public static UnpackedLevel UnpackLevel(string levelName, bool load = false)
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
            MajorVersion = levelData.MajorVersion,
            buildDate = levelData.buildDate,
            uniqueIdentifier = levelData.uniqueIdentifier,
            thumbnail = new Sprite(), // doing new Sprite() incase load is false but the level doesnt have a thumbnail
            savedObjects = []
        };

        /*if (load)
        {
            // we dont need the thumbnail if we're just loading the level, so just skip its bytes
            binaryReader.BaseStream.Seek(levelData.thumbnailSize, SeekOrigin.Current);

            if (levelData.calmThemeSize > 0)
            {
                byte[] music = binaryReader.ReadBytes(levelData.calmThemeSize);

                // temporarly write the music to a file so we can turn it into an audio clip (im sowwy idk any other way to turn audio bytes into an audioclip 3:)
                string tempFile = Path.Combine(Application.temporaryCachePath, levelData.calmThemeName);
                File.WriteAllBytes(tempFile, music);

                AudioType audType = levelData.calmThemeName.ToLower() switch
                {
                    "wav" => AudioType.WAV,
                    "ogg" => AudioType.OGGVORBIS,
                    "mp3" => AudioType.MPEG,
                    "mp4" => AudioType.MPEG,
                    _ => AudioType.UNKNOWN
                };
                var request = UnityWebRequestMultimedia.GetAudioClip("file:///" + tempFile, levelData.calmThemeName.ToLower() switch { "wav" => AudioType.WAV, "ogg" => AudioType.OGGVORBIS, "mp3" => AudioType.MPEG, "mp4" => AudioType.MPEG, _ => AudioType.UNKNOWN });
                var op = request.SendWebRequest();

                var tcs = new TaskCompletionSource<AsyncOperation>();
                op.completed += _ => tcs.SetResult(op);
                await tcs.Task; // turning it into a task since im too lazy to turn this whole thing into a coroutine

                if (request.result != UnityWebRequest.Result.Success)
                    Plugin.LogError($"Could not load level music, request error: {request.error}");
                else
                    unpacked.calmTheme = DownloadHandlerAudioClip.GetContent(request);

                File.Delete(tempFile);
            }

            if (levelData.battleThemeSize > 0)
            {
                byte[] music = binaryReader.ReadBytes(levelData.battleThemeSize);

                // temporarly write the music to a file so we can turn it into an audio clip (im sowwy idk any other way to turn audio bytes into an audioclip 3:)
                string tempFile = Path.Combine(Application.temporaryCachePath, levelData.battleThemeName);
                File.WriteAllBytes(tempFile, music);

                AudioType audType = levelData.battleThemeName.ToLower() switch
                {
                    "wav" => AudioType.WAV,
                    "ogg" => AudioType.OGGVORBIS,
                    "mp3" => AudioType.MPEG,
                    "mp4" => AudioType.MPEG,
                    _ => AudioType.UNKNOWN
                };
                var request = UnityWebRequestMultimedia.GetAudioClip("file:///" + tempFile, levelData.battleThemeName.ToLower() switch { "wav" => AudioType.WAV, "ogg" => AudioType.OGGVORBIS, "mp3" => AudioType.MPEG, "mp4" => AudioType.MPEG, _ => AudioType.UNKNOWN });
                var op = request.SendWebRequest();

                var tcs = new TaskCompletionSource<AsyncOperation>();
                op.completed += _ => tcs.SetResult(op);
                await tcs.Task; // turning it into a task since im too lazy to turn this whole thing into a coroutine

                if (request.result != UnityWebRequest.Result.Success)
                    Plugin.LogError($"Could not load level music, request error: {request.error}");
                else
                    unpacked.battleTheme = DownloadHandlerAudioClip.GetContent(request);

                File.Delete(tempFile);
            }

            unpacked.savedObjects = levelData.savedObjects;
        }
        else */if (levelData.thumbnailSize > 0)
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

    async void cSharp_script_UE()
    {
        await System.Threading.Tasks.Task.Delay(1500);

        string bundlePath = @"C:\Users\freda\ULTRAEDITOR\Assets\Bundles\emptyscene.bundle";
        UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile(bundlePath);

        string[] scenePaths = bundle.GetAllScenePaths();
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
        UnityEngine.Debug.Log($"Loading scene: {sceneName}");

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}