using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class ArenaObject : SavableObject
    {
        public List<string> enemyIds = new List<string>();
        public bool onlyWave = true;

        public static ArenaObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            ArenaObject arenaObject = target.AddComponent<ArenaObject>();
            if (spawnedObject != null) spawnedObject.arenaObject = arenaObject;
            return arenaObject;
        }

        public void addId(string id)
        {
            enemyIds.Add(id);
        }

        public void createArena()
        {
            if (gameObject.GetComponent<ActivateArena>() != null) { return; }
            ActivateArena activateArena = gameObject.AddComponent<ActivateArena>();
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            activateArena.doors = [];
            gameObject.GetComponent<Collider>().isTrigger = true;
            activateArena.onlyWave = onlyWave;

            activateArena.enemies = LoadingHelper.GetObjectsWithIds(enemyIds);

            bool enemiesHaveParent = true;
            foreach (var enemy in activateArena.enemies)
            {
                if (enemy.transform.parent == null || (enemy.transform.parent.GetComponent<CubeObject>() == null && enemy.transform.parent.GetComponent<ActivateNextWave>() == null)) enemiesHaveParent = false;
            }
            if (!enemiesHaveParent)
            {
                GameObject group = EditorManager.Instance.createCube(layer : "Invisible", objName : "EnemyWave", pos : activateArena.enemies[0].transform.position, matType: MaterialChoser.materialTypes.NoCollision);
                foreach (var enemy in activateArena.enemies)
                {
                    enemy.transform.SetParent(group.transform, true);
                }
            }
            if (activateArena.enemies.Length > 0)
                activateArena.enemies[0].transform.parent.gameObject.AddComponent<GoreZone>();
        }
    }
}
