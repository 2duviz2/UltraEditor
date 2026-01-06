using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.Editor;
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
            activateNextWave.nextEnemies = LoadingHelper.GetObjectsWithIds(enemyIds);
            activateNextWave.toActivate = LoadingHelper.GetObjectsWithIds(toActivateIds);
            activateNextWave.noActivationDelay = true;
            Destroy(gameObject.GetComponent<Collider>());
            activateNextWave.lastWave = lastWave;
            activateNextWave.enemyCount = enemyCount;
        }
    }
}
