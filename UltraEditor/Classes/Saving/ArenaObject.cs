using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    internal class ArenaObject : SavableObject
    {
        public List<string> enemyIds = new List<string>();

        public static ArenaObject Create(GameObject target)
        {
            ArenaObject obj = target.AddComponent<ArenaObject>();
            return obj;
        }

        public void addId(string id)
        {
            enemyIds.Add(id);
        }

        public void createArena()
        {
            ActivateArena activateArena = gameObject.AddComponent<ActivateArena>();
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            activateArena.doors = [];
            gameObject.GetComponent<Collider>().isTrigger = true;

            foreach (var e in enemyIds)
            {
                foreach (var obj in GameObject.FindObjectsOfType<SpawnedObject>(true))
                {
                    if (e == obj.ID)
                    {
                        List<GameObject> enemies = (activateArena.enemies ?? new GameObject[0]).ToList();
                        enemies.Add(obj.gameObject);
                        activateArena.enemies = enemies.ToArray();
                        break;
                    }
                }
            }
        }
    }
}
