using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    internal class NextArenaObject : SavableObject
    {
        public List<string> enemyIds = new List<string>();
        public List<string> toActivateIds = new List<string>();
        public bool lastWave = true;
        public int enemyCount = 0;

        public static NextArenaObject Create(GameObject target)
        {
            NextArenaObject obj = target.AddComponent<NextArenaObject>();
            return obj;
        }

        public void addEnemyId(string id)
        {
            enemyIds.Add(id);
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
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
                foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> enemies = (activateNextWave.nextEnemies ?? new GameObject[0]).ToList();
                        enemies.Add(obj.gameObject);
                        activateNextWave.nextEnemies = enemies.ToArray();
                        break;
                    }
                }
            }

            foreach (var e in toActivateIds)
            {
                foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> toActivate = (activateNextWave.toActivate ?? []).ToList();
                        toActivate.Add(obj.gameObject);
                        activateNextWave.toActivate = toActivate.ToArray();
                        break;
                    }
                }
            }
        }
    }
}
