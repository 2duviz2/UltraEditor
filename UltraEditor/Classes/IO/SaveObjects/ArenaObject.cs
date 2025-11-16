using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    internal class ArenaObject : SavableObject
    {
        public List<string> enemyIds = new List<string>();
        public bool onlyWave = true;

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
            activateArena.onlyWave = onlyWave;

            foreach (var e in enemyIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> enemies = (activateArena.enemies ?? []).ToList();
                        enemies.Add(obj.gameObject);
                        activateArena.enemies = enemies.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> enemies = (activateArena.enemies ?? []).ToList();
                            enemies.Add(obj.gameObject);
                            activateArena.enemies = enemies.ToArray();
                            found = true;
                            break;
                        }
                    }
            }
        }
    }
}
