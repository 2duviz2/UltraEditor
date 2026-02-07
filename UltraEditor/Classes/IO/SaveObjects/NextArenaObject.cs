namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

public class NextArenaObject : SavableObject
{
    public List<string> enemyIds = new List<string>();
    public List<string> toActivateIds = new List<string>();
    public bool lastWave = true;
    public int enemyCount = 0;

    public static NextArenaObject Create(GameObject target)
    {
        NextArenaObject nextArenaObject = target.AddComponent<NextArenaObject>();
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
    public override void Tick()
    {
        if (Time.timeScale == 0) return;
        if (!addedGoreZone)
            gameObject.AddComponent<GoreZone>();
        addedGoreZone = true;
    }

    public override void Create()
    {
        ActivateNextWave activateNextWave = gameObject.AddComponent<ActivateNextWave>();
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        activateNextWave.doors = [];
        activateNextWave.nextEnemies = LoadingHelper.GetObjectsWithIds(enemyIds);
        activateNextWave.toActivate = LoadingHelper.GetObjectsWithIds(toActivateIds);
        //activateNextWave.noActivationDelay = true; removed because its dumb :c
        Destroy(gameObject.GetComponent<Collider>());
        activateNextWave.lastWave = lastWave;
        activateNextWave.enemyCount = enemyCount;
    }
}
