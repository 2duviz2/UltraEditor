using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UltraEditor.Classes.Editor;
using UltraEditor.Classes.IO.SaveObjects;
using UltraEditor.Classes.World;
using UnityEngine;

namespace UltraEditor.Classes.IO
{
    public static class Saving
    {
        public static string addShit(SavableObject obj)
        {
            string text = "";

            obj.Update();
            text += LoadingHelper.GetIdOfObj(obj.gameObject);
            text += "\n";
            text += obj.name;
            text += "\n";
            text += obj.gameObject.layer;
            text += "\n";
            text += obj.gameObject.tag;
            text += "\n";
            text += obj.Position.ToString();
            text += "\n";
            text += obj.EulerAngles.ToString();
            text += "\n";
            text += obj.Scale.ToString();
            text += "\n";
            text += obj.gameObject.activeSelf ? 1 : 0;
            text += "\n";
            text += obj.transform.parent != null ? LoadingHelper.GetIdOfObj(obj.transform.parent.gameObject) : "";
            text += "\n";

            return text;
        }

        public static T[] ReverseArray<T>(T[] array)
        {
            if (array == null) return null;
            Array.Reverse(array);
            return array;
        }

        public static string GetSceneOld()
        {
            List<GameObject> iterated = [];
            string text = "";

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CubeObject>(true)))
            {
                if (obj.GetComponent<ActivateArena>() != null && obj.GetComponent<Collider>().isTrigger)
                {
                    GameObject ob = obj.gameObject;
                    GameObject.Destroy(obj.GetComponent<CubeObject>());
                    if (obj.GetComponent<ArenaObject>() == null)
                        ArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponent<ActivateNextWave>() != null)
                {
                    GameObject ob = obj.gameObject;
                    GameObject.Destroy(obj.GetComponent<CubeObject>());
                    NextArenaObject o = NextArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponents(typeof(Component)).Where(o => EditorComponentsList.GetTypes().Contains(o.GetType())).ToList().Count != 0)
                    continue;

                text += "? CubeObject ?";
                text += "\n";
                text += addShit(obj);
                text += (int)obj.matType + "\n";
                text += "? PASS ?\n";
                text += obj.matTiling + "\n";
                text += "? PASS ?\n";
                text += obj._isTrigger + "\n";
                text += "? PASS ?\n";
                text += (int)obj.shape + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<PrefabObject>(true)))
            {
                if (obj.GetComponent<CheckPoint>() != null) continue;
                if (obj.transform.parent != null && obj.transform.parent.name == "Automated Gore Zone") continue;
                if (obj.GetComponent<ItemIdentifier>() != null && obj.GetComponent<ItemIdentifier>().pickedUp) continue;
                text += "? PrefabObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.PrefabAsset + "\n";
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            iterated = [];
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<ArenaObject>(true)))
            {
                if (iterated.Contains(obj.gameObject)) continue;
                if (obj.GetComponent<ActivateArena>() == null) continue;
                obj.enemyIds.Clear();
                if (obj.GetComponent<ActivateArena>().enemies != null)
                    foreach (var e in obj.GetComponent<ActivateArena>().enemies)
                        if (e != null)
                            obj.addId(LoadingHelper.GetIdOfObj(e));

                text += "? ArenaObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.GetComponent<ActivateArena>().onlyWave + "\n";
                foreach (var e in obj.enemyIds)
                    text += e + "\n";
                text += "? END ?";
                text += "\n";
                iterated.Add(obj.gameObject);
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<NextArenaObject>(true)))
            {
                if (obj.GetComponent<ActivateNextWave>() == null) continue;
                obj.enemyIds.Clear();
                obj.toActivateIds.Clear();
                if (obj.GetComponent<ActivateNextWave>().nextEnemies != null)
                    foreach (var e in obj.GetComponent<ActivateNextWave>().nextEnemies)
                        if (e != null)
                            obj.addEnemyId(LoadingHelper.GetIdOfObj(e));
                if (obj.GetComponent<ActivateNextWave>().toActivate != null)
                    foreach (var e in obj.GetComponent<ActivateNextWave>().toActivate)
                        if (e != null)
                            obj.addToActivateId(LoadingHelper.GetIdOfObj(e));

                text += "? NextArenaObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.GetComponent<ActivateNextWave>().lastWave + "\n";
                text += obj.GetComponent<ActivateNextWave>().enemyCount + "\n";
                foreach (var e in obj.enemyIds)
                    text += e + "\n";
                text += "? PASS ?\n";
                foreach (var e in obj.toActivateIds)
                    text += e + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<ActivateObject>(true)))
            {
                obj.toActivateIds.Clear();
                obj.toDeactivateIds.Clear();
                foreach (var e in obj.toActivate)
                {
                    if (e != null)
                        obj.addToActivateId(LoadingHelper.GetIdOfObj(e));
                }
                foreach (var e in obj.toDeactivate)
                {
                    if (e != null)
                        obj.addtoDeactivateId(LoadingHelper.GetIdOfObj(e));
                }

                text += "? ActivateObject ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.toActivateIds)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in obj.toDeactivateIds)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                text += obj.canBeReactivated.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += obj.delay.ToString();
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<HUDMessageObject>(true)))
            {
                text += "? HUDMessageObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.message + "\n";
                text += "? PASS ?\n";
                text += obj.disableAfterShowing + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<IO.SaveObjects.TeleportObject>(true)))
            {
                text += "? TeleportObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.teleportPosition + "\n";
                text += "? PASS ?\n";
                text += obj.canBeReactivated + "\n";
                text += "? PASS ?\n";
                text += obj.slowdown + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<LevelInfoObject>(true)))
            {
                text += "? LevelInfoObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.ambientColor + "\n";
                text += "? PASS ?\n";
                text += obj.intensityMultiplier + "\n";
                text += "? PASS ?\n";
                text += obj.changeLighting + "\n";
                text += "? PASS ?\n";
                text += obj.tipOfTheDay + "\n";
                text += "? PASS ?\n";
                text += obj.levelLayer + "\n";
                text += "? PASS ?\n";
                text += obj.playMusicOnDoorOpen + "\n";
                text += "? PASS ?\n";
                text += obj.levelName + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CheckPoint>(true)))
            {
                while (obj.GetComponent<CheckpointObject>() != null)
                    GameObject.Destroy(obj.GetComponent<CheckpointObject>());

                CheckpointObject co = CheckpointObject.Create(obj.gameObject);

                foreach (var e in obj.rooms)
                {
                    if (e != null)
                    {
                        if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                            co.addRoomId(LoadingHelper.GetIdOfObj(e, new Vector3(-10000, 0, 0)));
                        else
                            co.addRoomId(LoadingHelper.GetIdOfObj(e));
                    }
                }
                foreach (var e in obj.roomsToInherit)
                {
                    if (e != null)
                        co.addRoomToInheritId(LoadingHelper.GetIdOfObj(e));
                }

                text += "? CheckpointObject ?";
                text += "\n";
                if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                    text += addShit(co.transform.parent.GetComponent<CheckpointObject>());
                else
                    text += addShit(co);
                foreach (var e in co.rooms)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in co.roomsToInherit)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";

                GameObject.Destroy(obj.GetComponent<CheckpointObject>());
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CheckpointObject>(true)))
            {
                if (obj.transform.childCount != 0) continue;

                obj.rooms = [];
                foreach (var e in obj.checkpointRooms)
                {
                    if (e != null)
                        obj.addRoomId(LoadingHelper.GetIdOfObj(e));
                }

                obj.roomsToInherit = [];
                foreach (var e in obj.checkpointRoomsToInherit)
                {
                    if (e != null)
                        obj.addRoomToInheritId(LoadingHelper.GetIdOfObj(e));
                }

                text += "? CheckpointObject ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.rooms)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in obj.roomsToInherit)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<DeathZone>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null || obj.GetComponent<PrefabObject>() != null) continue;
                text += "? DeathZone ?";
                text += "\n";
                text += addShit(obj.gameObject.AddComponent<SavableObject>());
                text += obj.notInstakill.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += obj.damage.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += (int)obj.affected;
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<Light>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                if (obj.GetComponent<PrefabObject>() != null) continue;
                text += "? Light ?";
                text += "\n";
                text += addShit(obj.gameObject.AddComponent<SavableObject>());
                text += obj.intensity.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += obj.range.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += (int)obj.type;
                text += "\n";
                text += "? PASS ?\n";
                text += (new Vector3(obj.color.r * 255f, obj.color.g * 255f, obj.color.b * 255f)).ToString();
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<MusicObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                text += "? MusicObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.calmThemePath;
                text += "\n";
                text += "? PASS ?\n";
                text += obj.battleThemePath;
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<SFXObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                text += "? SFXObject ?";
                text += "\n";
                text += addShit(obj);
                text += $"{obj.url}\n";
                text += "? PASS ?\n";
                text += $"{obj.disableAfterPlaying}\n";
                text += "? PASS ?\n";
                text += $"{obj.playOnAwake}\n";
                text += "? PASS ?\n";
                text += $"{obj.loop}\n";
                text += "? PASS ?\n";
                text += $"{obj.range}\n";
                text += "? PASS ?\n";
                text += $"{obj.volume}\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<MovingPlatformAnimator>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.affectedCubesIds = [];
                obj.pointsIds = [];
                foreach (var e in obj.affectedCubes)
                    if (e != null)
                        obj.addAffectedCubeId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.points)
                    if (e != null)
                        obj.addPointId(LoadingHelper.GetIdOfObj(e));
                text += "? MovingPlatformAnimator ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.affectedCubesIds)
                    text += e + "\n";
                text += "? PASS ?\n";
                foreach (var e in obj.pointsIds)
                    text += e + "\n";
                text += "? PASS ?\n";
                text += $"{obj.speed}\n";
                text += "? PASS ?\n";
                text += $"{obj.movesWithThePlayer}\n";
                text += "? PASS ?\n";
                text += $"{(int)obj.mode}\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<SkullActivatorObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.triggerAltarsIds = [];
                obj.toActivateIds = [];
                obj.toDeactivateIds = [];
                foreach (var e in obj.toActivate)
                    if (e != null)
                        obj.addToActivateId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.toDeactivate)
                    if (e != null)
                        obj.addToDeactivateId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.triggerAltars)
                    if (e != null)
                        obj.addTriggerAltarId(LoadingHelper.GetIdOfObj(e));
                text += "? SkullActivatorObject ?";
                text += "\n";
                text += addShit(obj);
                text += $"{(int)obj.acceptedItemType}\n";
                text += "? PASS ?\n";
                foreach (var e in obj.toActivateIds)
                    text += e + "\n";
                text += "? PASS ?\n";
                foreach (var e in obj.toDeactivateIds)
                    text += e + "\n";
                text += "? PASS ?\n";
                foreach (var e in obj.triggerAltarsIds)
                    text += e + "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CubeTilingAnimator>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.affectedCubesIds = [];
                foreach (var e in obj.affectedCubes)
                    if (e != null)
                        obj.addId(LoadingHelper.GetIdOfObj(e));
                text += "? CubeTilingAnimator ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.affectedCubesIds)
                    text += e + "\n";
                text += "\n";
                text += "? PASS ?\n";
                text += obj.scrolling.ToString();
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            return text;
        }
    }

    public static class SceneJsonSaver
    {
        class SceneSave
        {
            public List<SerializedObject> objects = new List<SerializedObject>();
        }

        class SerializedObject
        {
            public string type;
            public CommonFields common;
            public JObject data;
        }

        class CommonFields
        {
            public string id;
            public string name;
            public int layer;
            public string tag;
            public float[] position;
            public float[] eulerAngles;
            public float[] scale;
            public bool active;
            public string parentId;
        }

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };

        static float[] V3(Vector3 v) => new float[] { v.x, v.y, v.z };

        static CommonFields SerializeCommon(SavableObject obj)
        {
            obj.Update();
            var go = obj.gameObject;
            var cf = new CommonFields
            {
                id = LoadingHelper.GetIdOfObj(go),
                name = obj.name,
                layer = go.layer,
                tag = go.tag,
                position = V3(obj.Position),
                eulerAngles = V3(obj.EulerAngles),
                scale = V3(obj.Scale),
                active = go.activeSelf,
                parentId = obj.transform.parent != null ? LoadingHelper.GetIdOfObj(obj.transform.parent.gameObject) : null
            };
            return cf;
        }

        public static T[] ReverseArray<T>(T[] array)
        {
            if (array == null) return null;
            Array.Reverse(array);
            return array;
        }

        public static string GetSceneJson()
        {
            LoadingHelper.cachedIds = [];
            LoadingHelper.legacyIDs = false;

            Plugin.LogInfo("Creating scene...");
            var scene = new SceneSave();
            Plugin.LogInfo("Serializing objects...");

            // CubeObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CubeObject>(true)))
            {
                if (obj.GetComponent<ActivateArena>() != null && obj.GetComponent<Collider>().isTrigger)
                {
                    var ob = obj.gameObject;
                    GameObject.Destroy(obj.GetComponent<CubeObject>());
                    if (obj.GetComponent<ArenaObject>() == null)
                        ArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponent<ActivateNextWave>() != null)
                {
                    var ob = obj.gameObject;
                    GameObject.Destroy(obj.GetComponent<CubeObject>());
                    NextArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponents(typeof(Component)).Where(o => EditorComponentsList.GetTypes().Contains(o.GetType())).ToList().Count != 0)
                    continue;

                var so = new SerializedObject
                {
                    type = "CubeObject",
                    common = SerializeCommon(obj)
                };

                var data = new JObject();
                data["matType"] = (int)obj.matType;
                data["matTiling"] = obj.matTiling;
                data["isTrigger"] = obj._isTrigger;
                data["shape"] = (int)obj.shape;
                data["fixTiling"] = obj.fixMaterialTiling;
                so.data = data;
                scene.objects.Add(so);
            }

            // PrefabObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<PrefabObject>(true)))
            {
                if (obj.GetComponent<CheckPoint>() != null) continue;
                if (obj.transform.parent != null && obj.transform.parent.name == "Automated Gore Zone") continue;
                if (obj.GetComponent<ItemIdentifier>() != null && obj.GetComponent<ItemIdentifier>().pickedUp) continue;

                var so = new SerializedObject
                {
                    type = "PrefabObject",
                    common = SerializeCommon(obj)
                };

                var data = new JObject();
                if (!string.IsNullOrEmpty(obj.PrefabAsset)) data["prefabAsset"] = obj.PrefabAsset;
                so.data = data;
                scene.objects.Add(so);
            }

            // ArenaObject
            {
                var iterated = new HashSet<GameObject>();
                foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<ArenaObject>(true)))
                {
                    if (iterated.Contains(obj.gameObject)) continue;
                    if (obj.GetComponent<ActivateArena>() == null) continue;

                    var activate = obj.GetComponent<ActivateArena>();
                    obj.enemyIds.Clear();
                    if (activate.enemies != null)
                    {
                        foreach (var e in activate.enemies)
                            if (e != null)
                                obj.addId(LoadingHelper.GetIdOfObj(e));
                    }

                    var so = new SerializedObject
                    {
                        type = "ArenaObject",
                        common = SerializeCommon(obj)
                    };

                    var data = new JObject();
                    data["onlyWave"] = activate.onlyWave;
                    var arr = new JArray();
                    foreach (var eId in obj.enemyIds) arr.Add(eId);
                    if (arr.Count > 0) data["enemyIds"] = arr;

                    so.data = data;
                    scene.objects.Add(so);
                    iterated.Add(obj.gameObject);
                }
            }

            // NextArenaObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<NextArenaObject>(true)))
            {
                if (obj.GetComponent<ActivateNextWave>() == null) continue;
                var an = obj.GetComponent<ActivateNextWave>();
                obj.enemyIds.Clear();
                obj.toActivateIds.Clear();
                if (an.nextEnemies != null)
                    foreach (var e in an.nextEnemies)
                        if (e != null)
                            obj.addEnemyId(LoadingHelper.GetIdOfObj(e));
                if (an.toActivate != null)
                    foreach (var e in an.toActivate)
                        if (e != null)
                            obj.addToActivateId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "NextArenaObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["lastWave"] = an.lastWave;
                data["enemyCount"] = an.enemyCount;
                if (obj.enemyIds != null && obj.enemyIds.Count > 0) data["enemyIds"] = new JArray(obj.enemyIds);
                if (obj.toActivateIds != null && obj.toActivateIds.Count > 0) data["toActivateIds"] = new JArray(obj.toActivateIds);
                so.data = data;
                scene.objects.Add(so);
            }

            // ActivateObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<ActivateObject>(true)))
            {
                obj.toActivateIds.Clear();
                obj.toDeactivateIds.Clear();
                foreach (var e in obj.toActivate)
                    if (e != null) obj.addToActivateId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.toDeactivate)
                    if (e != null) obj.addtoDeactivateId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "ActivateObject", common = SerializeCommon(obj) };
                var data = new JObject();
                if (obj.toActivateIds.Count > 0) data["toActivateIds"] = new JArray(obj.toActivateIds);
                if (obj.toDeactivateIds.Count > 0) data["toDeactivateIds"] = new JArray(obj.toDeactivateIds);
                data["canBeReactivated"] = obj.canBeReactivated;
                data["delay"] = obj.delay;
                so.data = data;
                scene.objects.Add(so);
            }

            // HUDMessageObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<HUDMessageObject>(true)))
            {
                var so = new SerializedObject { type = "HUDMessageObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["message"] = obj.message;
                data["disableAfterShowing"] = obj.disableAfterShowing;
                so.data = data;
                scene.objects.Add(so);
            }

            // TeleportObject (IO.SaveObjects)
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<IO.SaveObjects.TeleportObject>(true)))
            {
                var so = new SerializedObject { type = "TeleportObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["teleportPosition"] = JArray.FromObject(V3(obj.teleportPosition));
                data["canBeReactivated"] = obj.canBeReactivated;
                data["slowdown"] = obj.slowdown;
                so.data = data;
                scene.objects.Add(so);
            }

            // LevelInfoObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<LevelInfoObject>(true)))
            {
                var so = new SerializedObject { type = "LevelInfoObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["ambientColor"] = JArray.FromObject(V3(obj.ambientColor));
                data["intensityMultiplier"] = obj.intensityMultiplier;
                data["changeLighting"] = obj.changeLighting;
                data["tipOfTheDay"] = obj.tipOfTheDay;
                data["levelLayer"] = obj.levelLayer;
                data["playMusicOnDoorOpen"] = obj.playMusicOnDoorOpen;
                data["levelName"] = obj.levelName;
                data["skybox"] = (int)obj.skybox;
                so.data = data;
                scene.objects.Add(so);
            }

            // CheckPoint/CheckpointObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CheckPoint>(true)))
            {
                while (obj.GetComponent<CheckpointObject>() != null)
                    GameObject.Destroy(obj.GetComponent<CheckpointObject>());

                var co = CheckpointObject.Create(obj.gameObject);

                foreach (var e in obj.rooms)
                    if (e != null)
                    {
                        if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                            co.addRoomId(LoadingHelper.GetIdOfObj(e, new Vector3(-10000, 0, 0)));
                        else
                            co.addRoomId(LoadingHelper.GetIdOfObj(e));
                    }

                foreach (var e in obj.roomsToInherit)
                    if (e != null) co.addRoomToInheritId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "CheckpointObject", common = SerializeCommon(co) };
                var data = new JObject();
                if (co.rooms.Count > 0) data["rooms"] = new JArray(co.rooms);
                if (co.roomsToInherit.Count > 0) data["roomsToInherit"] = new JArray(co.roomsToInherit);
                so.data = data;
                scene.objects.Add(so);

                GameObject.Destroy(obj.GetComponent<CheckpointObject>());
            }

            // CheckpointObject children-less cases
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CheckpointObject>(true)))
            {
                if (obj.transform.childCount != 0) continue;

                obj.rooms = new List<string>();
                foreach (var e in obj.checkpointRooms)
                    if (e != null) obj.addRoomId(LoadingHelper.GetIdOfObj(e));

                obj.roomsToInherit = new List<string>();
                foreach (var e in obj.checkpointRoomsToInherit)
                    if (e != null) obj.addRoomToInheritId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "CheckpointObject", common = SerializeCommon(obj) };
                var data = new JObject();
                if (obj.rooms.Count > 0) data["rooms"] = new JArray(obj.rooms);
                if (obj.roomsToInherit.Count > 0) data["roomsToInherit"] = new JArray(obj.roomsToInherit);
                so.data = data;
                scene.objects.Add(so);
            }

            // DeathZone
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<DeathZone>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null || obj.GetComponent<PrefabObject>() != null) continue;
                var sav = obj.gameObject.AddComponent<SavableObject>();
                var so = new SerializedObject { type = "DeathZone", common = SerializeCommon(sav) };
                var data = new JObject();
                data["notInstakill"] = obj.notInstakill;
                data["damage"] = obj.damage;
                data["affected"] = (int)obj.affected;
                so.data = data;
                scene.objects.Add(so);
            }

            // Light
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<Light>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                if (obj.GetComponent<PrefabObject>() != null) continue;
                var sav = obj.gameObject.AddComponent<SavableObject>();
                var so = new SerializedObject { type = "Light", common = SerializeCommon(sav) };
                var data = new JObject();
                data["intensity"] = obj.intensity;
                data["range"] = obj.range;
                data["lightType"] = (int)obj.type;
                var colorVec = new Vector3(obj.color.r * 255f, obj.color.g * 255f, obj.color.b * 255f);
                data["colorRGB255"] = JArray.FromObject(V3(colorVec));
                so.data = data;
                scene.objects.Add(so);
            }

            // MusicObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<MusicObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                var so = new SerializedObject { type = "MusicObject", common = SerializeCommon(obj) };
                var data = new JObject();
                if (!string.IsNullOrEmpty(obj.calmThemePath)) data["calmThemePath"] = obj.calmThemePath;
                if (!string.IsNullOrEmpty(obj.battleThemePath)) data["battleThemePath"] = obj.battleThemePath;
                so.data = data;
                scene.objects.Add(so);
            }

            // SFXObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<SFXObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                var so = new SerializedObject { type = "SFXObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["url"] = obj.url;
                data["disableAfterPlaying"] = obj.disableAfterPlaying;
                data["playOnAwake"] = obj.playOnAwake;
                data["loop"] = obj.loop;
                data["range"] = obj.range;
                data["volume"] = obj.volume;
                so.data = data;
                scene.objects.Add(so);
            }

            // MovingPlatformAnimator
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<MovingPlatformAnimator>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.affectedCubesIds = new List<string>();
                obj.pointsIds = new List<string>();
                foreach (var e in obj.affectedCubes)
                    if (e != null) obj.addAffectedCubeId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.points)
                    if (e != null) obj.addPointId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "MovingPlatformAnimator", common = SerializeCommon(obj) };
                var data = new JObject();
                if (obj.affectedCubesIds.Count > 0) data["affectedCubesIds"] = new JArray(obj.affectedCubesIds);
                if (obj.pointsIds.Count > 0) data["pointsIds"] = new JArray(obj.pointsIds);
                data["speed"] = obj.speed;
                data["movesWithThePlayer"] = obj.movesWithThePlayer;
                data["mode"] = (int)obj.mode;
                so.data = data;
                scene.objects.Add(so);
            }

            // SkullActivatorObject
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<SkullActivatorObject>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.triggerAltarsIds = new List<string>();
                obj.toActivateIds = new List<string>();
                obj.toDeactivateIds = new List<string>();
                foreach (var e in obj.toActivate) if (e != null) obj.addToActivateId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.toDeactivate) if (e != null) obj.addToDeactivateId(LoadingHelper.GetIdOfObj(e));
                foreach (var e in obj.triggerAltars) if (e != null) obj.addTriggerAltarId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "SkullActivatorObject", common = SerializeCommon(obj) };
                var data = new JObject();
                data["acceptedItemType"] = (int)obj.acceptedItemType;
                if (obj.toActivateIds.Count > 0) data["toActivateIds"] = new JArray(obj.toActivateIds);
                if (obj.toDeactivateIds.Count > 0) data["toDeactivateIds"] = new JArray(obj.toDeactivateIds);
                if (obj.triggerAltarsIds.Count > 0) data["triggerAltarsIds"] = new JArray(obj.triggerAltarsIds);
                so.data = data;
                scene.objects.Add(so);
            }

            // CubeTilingAnimator
            foreach (var obj in ReverseArray(GameObject.FindObjectsOfType<CubeTilingAnimator>(true)))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                obj.affectedCubesIds = new List<string>();
                foreach (var e in obj.affectedCubes) if (e != null) obj.addId(LoadingHelper.GetIdOfObj(e));

                var so = new SerializedObject { type = "CubeTilingAnimator", common = SerializeCommon(obj) };
                var data = new JObject();
                if (obj.affectedCubesIds.Count > 0) data["affectedCubesIds"] = new JArray(obj.affectedCubesIds);
                data["scrolling"] = JArray.FromObject(V3(obj.scrolling));
                data["scrollingEnabled"] = obj.scrolling != Vector2.zero;
                so.data = data;
                scene.objects.Add(so);
            }

            return JsonConvert.SerializeObject(scene, jsonSettings);
        }

        public static void LoadSceneJson(string json)
        {
            Plugin.LogInfo("Trying to load scene json...");
            float startTime = Time.realtimeSinceStartup;

            if (string.IsNullOrEmpty(json))
            {
                Plugin.LogInfo("Empty json, aborting.");
                return;
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Failed to parse scene json: {ex}");
                return;
            }

            var objects = root["objects"] as JArray;
            if (objects == null)
            {
                Plugin.LogInfo("No objects array found in json.");
                return;
            }

            var createdSpawnedObjects = new List<SpawnedObject>();

            Vector3 ParseV3(JToken t)
            {
                if (t == null) return Vector3.zero;
                if (t.Type == JTokenType.Array)
                {
                    var a = t as JArray;
                    float x = a.Count > 0 ? (float)a[0] : 0f;
                    float y = a.Count > 1 ? (float)a[1] : 0f;
                    float z = a.Count > 2 ? (float)a[2] : 0f;
                    return new Vector3(x, y, z);
                }
                var s = t.ToString();
                if (!string.IsNullOrEmpty(s))
                    return ParseHelper.ParseVector3(s);
                return Vector3.zero;
            }
            Vector2 ParseV2(JToken t)
            {
                if (t == null) return Vector2.zero;
                if (t.Type == JTokenType.Array)
                {
                    var a = t as JArray;
                    float x = a.Count > 0 ? (float)a[0] : 0f;
                    float y = a.Count > 1 ? (float)a[1] : 0f;
                    return new Vector2(x, y);
                }
                var s = t.ToString();
                if (!string.IsNullOrEmpty(s))
                    return ParseHelper.ParseVector2(s);
                return Vector2.zero;
            }
            bool ParseBool(JToken t)
            {
                if (t == null) return false;
                if (t.Type == JTokenType.Boolean) return (bool)t;
                var s = t.ToString().ToLowerInvariant();
                return s == "true" || s == "1";
            }
            float ParseFloat(JToken t)
            {
                if (t == null) return 0f;
                if (t.Type == JTokenType.Float || t.Type == JTokenType.Integer) return (float)t;
                float.TryParse(t.ToString(), out var v);
                return v;
            }
            int ParseInt(JToken t)
            {
                if (t == null) return 0;
                if (t.Type == JTokenType.Integer) return (int)t;
                int.TryParse(t.ToString(), out var v);
                return v;
            }
            string ParseString(JToken t) => t?.ToString();

            foreach (var item in objects)
            {
                try
                {
                    var typeName = item["type"]?.ToString();
                    var common = item["common"] as JObject;
                    var data = item["data"] as JObject;

                    GameObject workingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    workingObject.AddComponent<SpawnedObject>();
                    var spawned = workingObject.GetComponent<SpawnedObject>();
                    createdSpawnedObjects.Add(spawned);

                    if (common != null)
                    {
                        if (common.TryGetValue("id", out var idTok)) spawned.ID = idTok.ToString();
                        if (common.TryGetValue("name", out var nameTok)) workingObject.name = nameTok.ToString();
                        if (common.TryGetValue("layer", out var layerTok)) workingObject.layer = ParseInt(layerTok);
                        if (common.TryGetValue("tag", out var tagTok)) workingObject.tag = tagTok.ToString();
                        if (common.TryGetValue("position", out var posTok)) workingObject.transform.position = ParseV3(posTok);
                        if (common.TryGetValue("eulerAngles", out var eulTok)) workingObject.transform.eulerAngles = ParseV3(eulTok);
                        if (common.TryGetValue("scale", out var scTok)) workingObject.transform.localScale = ParseV3(scTok);
                        if (common.TryGetValue("active", out var actTok)) workingObject.SetActive(ParseBool(actTok));
                        if (common.TryGetValue("parentId", out var pTok)) spawned.parentID = pTok?.ToString();
                    }

                    if (typeName == "CubeObject")
                    {
                        if (data != null && data.TryGetValue("matType", out var matTypeTok))
                        {
                            int mt = ParseInt(matTypeTok);
                            CubeObject.Create(workingObject, (MaterialChoser.materialTypes)Enum.GetValues(typeof(MaterialChoser.materialTypes)).GetValue(mt));
                        }
                        var cube = workingObject.GetComponent<CubeObject>();
                        if (cube != null && data != null)
                        {
                            if (data.TryGetValue("matTiling", out var mtil)) cube.matTiling = ParseFloat(mtil);
                            if (data.TryGetValue("isTrigger", out var istr)) cube.isTrigger = ParseBool(istr);
                            if (data.TryGetValue("shape", out var shp)) cube.shape = (MaterialChoser.shapes)Enum.GetValues(typeof(MaterialChoser.shapes)).GetValue(ParseInt(shp));
                            if (data.TryGetValue("fixTiling", out var fx)) cube.fixMaterialTiling = ParseBool(fx);
                        }
                    }
                    else if (typeName == "PrefabObject")
                    {
                        if (data != null && data.TryGetValue("prefabAsset", out var pTok))
                        {
                            var prefabName = pTok.ToString();
                            GameObject newObj = EditorManager.Instance.SpawnAsset(prefabName, true);
                            if (newObj != null)
                            {
                                newObj.transform.position = workingObject.transform.position;
                                newObj.transform.eulerAngles = workingObject.transform.eulerAngles;
                                newObj.transform.localScale = workingObject.transform.localScale;
                                newObj.layer = workingObject.layer;
                                newObj.tag = workingObject.tag;
                                newObj.name = workingObject.name;
                                newObj.SetActive(workingObject.activeSelf);
                                newObj.AddComponent<SpawnedObject>();
                                var newSpawned = newObj.GetComponent<SpawnedObject>();
                                newSpawned.ID = spawned.ID;
                                newSpawned.parentID = spawned.parentID;
                                var doorComp = newObj.GetComponent<Door>();
                                if (doorComp != null)
                                {
                                    var m = doorComp.GetType().GetMethod("GetPos", BindingFlags.Instance | BindingFlags.NonPublic);
                                    m?.Invoke(doorComp, null);
                                }
                                createdSpawnedObjects.Remove(spawned);
                                GameObject.DestroyImmediate(workingObject);
                                spawned = newSpawned;
                                workingObject = newObj;
                                createdSpawnedObjects.Add(spawned);
                            }
                        }
                    }
                    else if (typeName == "ArenaObject")
                    {
                        ArenaObject.Create(workingObject);
                        var ao = workingObject.GetComponent<ArenaObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("onlyWave", out var onlyTok)) ao.onlyWave = ParseBool(onlyTok);
                            if (data.TryGetValue("enemyIds", out var arr))
                            {
                                foreach (var e in (JArray)arr) if (e != null) ao.addId(e.ToString());
                            }
                        }
                    }
                    else if (typeName == "NextArenaObject")
                    {
                        NextArenaObject.Create(workingObject);
                        var na = workingObject.GetComponent<NextArenaObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("lastWave", out var lw)) na.lastWave = ParseBool(lw);
                            if (data.TryGetValue("enemyCount", out var ec)) na.enemyCount = ParseInt(ec);
                            if (data.TryGetValue("enemyIds", out var earr))
                            {
                                foreach (var e in (JArray)earr) if (e != null) na.addEnemyId(e.ToString());
                            }
                            if (data.TryGetValue("toActivateIds", out var tarr))
                            {
                                foreach (var e in (JArray)tarr) if (e != null) na.addToActivateId(e.ToString());
                            }
                        }
                    }
                    else if (typeName == "ActivateObject")
                    {
                        if (workingObject.GetComponent<ActivateObject>() == null) ActivateObject.Create(workingObject);
                        var ao = workingObject.GetComponent<ActivateObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("toActivateIds", out var tA))
                                foreach (var e in (JArray)tA) if (e != null) ao.addToActivateId(e.ToString());
                            if (data.TryGetValue("toDeactivateIds", out var tD))
                                foreach (var e in (JArray)tD) if (e != null) ao.addtoDeactivateId(e.ToString());
                            if (data.TryGetValue("canBeReactivated", out var c)) ao.canBeReactivated = ParseBool(c);
                            if (data.TryGetValue("delay", out var d)) ao.delay = ParseFloat(d);
                        }
                    }
                    else if (typeName == "CheckpointObject")
                    {
                        if (workingObject.GetComponent<CheckpointObject>() == null) CheckpointObject.Create(workingObject);
                        var co = workingObject.GetComponent<CheckpointObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("rooms", out var rArr))
                                foreach (var e in (JArray)rArr) if (e != null) co.addRoomId(e.ToString());
                            if (data.TryGetValue("roomsToInherit", out var riArr))
                                foreach (var e in (JArray)riArr) if (e != null) co.addRoomToInheritId(e.ToString());
                        }
                    }
                    else if (typeName == "DeathZone")
                    {
                        if (workingObject.GetComponent<DeathZoneObject>() == null) DeathZoneObject.Create(workingObject);
                        var dz = workingObject.GetComponent<DeathZoneObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("notInstakill", out var n)) dz.notInstaKill = ParseBool(n);
                            if (data.TryGetValue("damage", out var dmg)) dz.damage = ParseInt(dmg);
                            if (data.TryGetValue("affected", out var aff)) dz.affected = (AffectedSubjects)Enum.GetValues(typeof(AffectedSubjects)).GetValue(ParseInt(aff));
                        }
                    }
                    else if (typeName == "Light")
                    {
                        if (workingObject.GetComponent<LightObject>() == null) LightObject.Create(workingObject);
                        var lo = workingObject.GetComponent<LightObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("intensity", out var it)) lo.intensity = ParseFloat(it);
                            if (data.TryGetValue("range", out var rg)) lo.range = ParseFloat(rg);
                            if (data.TryGetValue("lightType", out var lt)) lo.type = (LightType)Enum.GetValues(typeof(LightType)).GetValue(ParseInt(lt));
                            if (data.TryGetValue("colorRGB255", out var col))
                            {
                                lo.color = ParseV3(col);
                            }
                        }
                    }
                    else if (typeName == "MusicObject")
                    {
                        if (workingObject.GetComponent<MusicObject>() == null) MusicObject.Create(workingObject);
                        var mo = workingObject.GetComponent<MusicObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("calmThemePath", out var c)) mo.calmThemePath = c.ToString();
                            if (data.TryGetValue("battleThemePath", out var b)) mo.battleThemePath = b.ToString();
                        }
                    }
                    else if (typeName == "SFXObject")
                    {
                        if (workingObject.GetComponent<SFXObject>() == null) SFXObject.Create(workingObject);
                        var sfx = workingObject.GetComponent<SFXObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("url", out var u)) sfx.url = u.ToString();
                            if (data.TryGetValue("disableAfterPlaying", out var d)) sfx.disableAfterPlaying = ParseBool(d);
                            if (data.TryGetValue("playOnAwake", out var p)) sfx.playOnAwake = ParseBool(p);
                            if (data.TryGetValue("loop", out var l)) sfx.loop = ParseBool(l);
                            if (data.TryGetValue("range", out var r)) sfx.range = ParseFloat(r);
                            if (data.TryGetValue("volume", out var v)) sfx.volume = ParseFloat(v);
                        }
                    }
                    else if (typeName == "MovingPlatformAnimator")
                    {
                        if (workingObject.GetComponent<MovingPlatformAnimator>() == null) MovingPlatformAnimator.Create(workingObject);
                        var mp = workingObject.GetComponent<MovingPlatformAnimator>();
                        if (data != null)
                        {
                            if (data.TryGetValue("affectedCubesIds", out var ac)) foreach (var e in (JArray)ac) if (e != null) mp.addAffectedCubeId(e.ToString());
                            if (data.TryGetValue("pointsIds", out var pts)) foreach (var e in (JArray)pts) if (e != null) mp.addPointId(e.ToString());
                            if (data.TryGetValue("speed", out var sp)) mp.speed = ParseFloat(sp);
                            if (data.TryGetValue("movesWithThePlayer", out var m)) mp.movesWithThePlayer = ParseBool(m);
                            if (data.TryGetValue("mode", out var md)) mp.mode = (MovingPlatformAnimator.platformMode)Enum.GetValues(typeof(MovingPlatformAnimator.platformMode)).GetValue(ParseInt(md));
                        }
                    }
                    else if (typeName == "SkullActivatorObject")
                    {
                        if (workingObject.GetComponent<SkullActivatorObject>() == null) SkullActivatorObject.Create(workingObject);
                        var sk = workingObject.GetComponent<SkullActivatorObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("acceptedItemType", out var at)) sk.acceptedItemType = (SkullActivatorObject.skullType)Enum.GetValues(typeof(SkullActivatorObject.skullType)).GetValue(ParseInt(at));
                            if (data.TryGetValue("toActivateIds", out var ta)) foreach (var e in (JArray)ta) if (e != null) sk.addToActivateId(e.ToString());
                            if (data.TryGetValue("toDeactivateIds", out var td)) foreach (var e in (JArray)td) if (e != null) sk.addToDeactivateId(e.ToString());
                            if (data.TryGetValue("triggerAltarsIds", out var tr)) foreach (var e in (JArray)tr) if (e != null) sk.addTriggerAltarId(e.ToString());
                        }
                    }
                    else if (typeName == "CubeTilingAnimator")
                    {
                        if (workingObject.GetComponent<CubeTilingAnimator>() == null) CubeTilingAnimator.Create(workingObject);
                        var ct = workingObject.GetComponent<CubeTilingAnimator>();
                        if (data != null)
                        {
                            if (data.TryGetValue("affectedCubesIds", out var ac)) foreach (var e in (JArray)ac) if (e != null) ct.addId(e.ToString());
                            if (data.TryGetValue("scrolling", out var sc)) ct.scrolling = ParseV3(sc);
                        }
                    }
                    else if (typeName == "HUDMessageObject")
                    {
                        if (workingObject.GetComponent<HUDMessageObject>() == null) HUDMessageObject.Create(workingObject);
                        var hm = workingObject.GetComponent<HUDMessageObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("message", out var m)) hm.message = m.ToString();
                            if (data.TryGetValue("disableAfterShowing", out var d)) hm.disableAfterShowing = ParseBool(d);
                        }
                    }
                    else if (typeName == "TeleportObject")
                    {
                        if (workingObject.GetComponent<IO.SaveObjects.TeleportObject>() == null) IO.SaveObjects.TeleportObject.Create(workingObject);
                        var tp = workingObject.GetComponent<IO.SaveObjects.TeleportObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("teleportPosition", out var tpos)) tp.teleportPosition = ParseV3(tpos);
                            if (data.TryGetValue("canBeReactivated", out var c)) tp.canBeReactivated = ParseBool(c);
                            if (data.TryGetValue("slowdown", out var s)) tp.slowdown = ParseBool(s);
                        }
                    }
                    else if (typeName == "LevelInfoObject")
                    {
                        if (workingObject.GetComponent<LevelInfoObject>() == null) LevelInfoObject.Create(workingObject);
                        var li = workingObject.GetComponent<LevelInfoObject>();
                        if (data != null)
                        {
                            if (data.TryGetValue("ambientColor", out var ac)) li.ambientColor = ParseV3(ac);
                            if (data.TryGetValue("intensityMultiplier", out var im)) li.intensityMultiplier = ParseFloat(im);
                            if (data.TryGetValue("changeLighting", out var cl)) li.changeLighting = ParseBool(cl);
                            if (data.TryGetValue("tipOfTheDay", out var tt)) li.tipOfTheDay = tt.ToString();
                            if (data.TryGetValue("levelLayer", out var ll)) li.levelLayer = ll.ToString();
                            if (data.TryGetValue("playMusicOnDoorOpen", out var pd)) li.playMusicOnDoorOpen = ParseBool(pd);
                            if (data.TryGetValue("levelName", out var ln)) li.levelName = ln.ToString();
                            if (data.TryGetValue("skybox", out var sx)) li.skybox = (SkyboxManager.Skybox)Enum.GetValues(typeof(SkyboxManager.Skybox)).GetValue(ParseInt(sx)); ;
                        }
                    }
                    else
                    {
                        try
                        {
                            var savType = Type.GetType(typeName) ?? Type.GetType(typeName + ", Assembly-CSharp");
                            if (savType != null)
                            {
                                var createMethod = savType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                                if (createMethod != null)
                                {
                                    createMethod.Invoke(null, new object[] { workingObject });
                                }
                                else
                                {
                                    workingObject.AddComponent(savType);
                                }

                                if (data != null)
                                {
                                    var comp = workingObject.GetComponent(savType);
                                    if (comp != null)
                                    {
                                        var fields = savType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                                        foreach (var f in fields)
                                        {
                                            if (data.TryGetValue(f.Name, out var tok))
                                            {
                                                try
                                                {
                                                    if (f.FieldType == typeof(Vector3)) f.SetValue(comp, ParseV3(tok));
                                                    else if (f.FieldType == typeof(Vector2)) f.SetValue(comp, ParseV2(tok));
                                                    else if (f.FieldType == typeof(bool)) f.SetValue(comp, ParseBool(tok));
                                                    else if (f.FieldType == typeof(int)) f.SetValue(comp, ParseInt(tok));
                                                    else if (f.FieldType == typeof(float)) f.SetValue(comp, ParseFloat(tok));
                                                    else f.SetValue(comp, tok.ToObject(f.FieldType));
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e) { Plugin.LogError(e); }
                    }
                }
                catch (Exception exItem)
                {
                    Plugin.LogError($"Error creating object: {exItem}");
                }
            }

            Plugin.LogInfo("Assigning parents...");
            var allObjs = GameObject.FindObjectsOfType<SpawnedObject>(true);
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

            Plugin.LogInfo("Creating objects...");
            foreach (var obj in allObjs)
            {
                try
                {
                    obj.GetComponent<SavableObject>()?.Create();
                }
                catch (Exception ex)
                {
                    Plugin.LogError($"Error calling Create() on object {obj.name}: {ex}");
                }
            }

            Plugin.LogInfo($"Loading done in {Time.realtimeSinceStartup - startTime} seconds!");

            try
            {
                EditorManager.Instance.cameraSelector.selectedObject = null;
                EditorManager.Instance.cameraSelector.UnselectObject();
            }
            catch { }
        }
    }
}