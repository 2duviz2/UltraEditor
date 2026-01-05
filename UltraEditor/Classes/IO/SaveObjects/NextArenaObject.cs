using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class NextArenaObject : SavableObject
    {
        public List<string> enemyIds = new List<string>();
        public List<string> toActivateIds = new List<string>();
        public bool lastWave = true;
        public int enemyCount = 0;

        public static NextArenaObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            NextArenaObject nextArenaObject = target.AddComponent<NextArenaObject>();
            if (spawnedObject != null) spawnedObject.nextArenaObject = nextArenaObject;
            return nextArenaObject;
        }

        public void addEnemyId(string id)
        {
            enemyIds.Add(id);
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
        }

        bool addedGoreZone = false;
        public void Tick()
        {
            if (Time.timeScale == 0) return;
            if (!addedGoreZone)
                gameObject.AddComponent<GoreZone>();
            addedGoreZone = true;
        }

        public void createArena()
        {
            ActivateNextWave activateNextWave = gameObject.AddComponent<ActivateNextWave>();
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            activateNextWave.doors = [];
            activateNextWave.nextEnemies = [];
            activateNextWave.toActivate = [];
            Destroy(gameObject.GetComponent<Collider>());
            activateNextWave.lastWave = lastWave;
            activateNextWave.enemyCount = enemyCount;

            foreach (var e in enemyIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> enemies = (activateNextWave.nextEnemies ?? new GameObject[0]).ToList();
                        enemies.Add(obj.gameObject);
                        activateNextWave.nextEnemies = enemies.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> enemies = (activateNextWave.nextEnemies ?? new GameObject[0]).ToList();
                            enemies.Add(obj.gameObject);
                            activateNextWave.nextEnemies = enemies.ToArray();
                            found = true;
                            break;
                        }
                    }
            }

            foreach (var item in toActivateIds)
            {
                if (EditorManager.logShit)
                    Plugin.LogInfo(item);
            }
            foreach (var e in toActivateIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        if (EditorManager.logShit)
                            Plugin.LogInfo($"FOUND OBEJCT {obj.name}");
                        List<GameObject> toActivate = (activateNextWave.toActivate ?? []).ToList();
                        toActivate.Add(obj.gameObject);
                        activateNextWave.toActivate = toActivate.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            if (EditorManager.logShit)
                                Plugin.LogInfo($"FOUND OBEJCT {obj.name}");
                            List<GameObject> toActivate = (activateNextWave.toActivate ?? []).ToList();
                            toActivate.Add(obj.gameObject);
                            activateNextWave.toActivate = toActivate.ToArray();
                            found = true;
                            break;
                        }
                    }
            }
        }
    }
}
