namespace UltraEditor.Classes.IO.SaveObjects;

using HarmonyLib;
using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using UltraEditor.Classes.TempScripts;
using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Sets the stats for every enemy in <b>Affected enemies</b>.")]
public class EnemyModifier : SavableObject
{
    [EditorVar("Affected enemies")]
    public GameObject[] affectedEnemies = [];

    public List<string> affectedEnemiesIds = [];

    [EditorVar("Sandified")]
    public bool sandified = false;

    [EditorVar("Attack enemies")]
    public bool attackEnemies = false;

    [EditorVar("Ignore player")]
    public bool ignorePlayer = false;

    [EditorVar("Radiance")]
    public float radiance = 0;

    [EditorVar("Has boss bar")]
    public bool boss = false;

    [EditorVar("Boss name")]
    public string bossName = "";

    public static EnemyModifier Create(GameObject target) =>
        target.AddComponent<EnemyModifier>();

    bool done = false;

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        affectedEnemies = LoadingHelper.GetObjectsWithIds(affectedEnemiesIds);
        done = false;
    }

    public override void Tick()
    {
        if (Time.timeScale == 0 || EditorManager.Instance.editorOpen) return;

        if (!done)
        {
            done = true;
            foreach (var obj in affectedEnemies)
            {
                if (obj == null) continue;
                var e = obj.GetComponent<EnemyIdentifier>() ?? obj.GetComponentInChildren<EnemyIdentifier>(true);
                if (e != null)
                {
                    if (radiance != 0)
                        e.gameObject.AddComponent<RadianceWaiter>().radianceTier = radiance;
                    e.sandified = sandified;
                    e.attackEnemies = attackEnemies;
                    e.prioritizeEnemiesUnlessAttacked = attackEnemies;
                    e.ignorePlayer = ignorePlayer;
                    e.BossBar(boss);
                    if (!string.IsNullOrEmpty(bossName))
                    {
                        var field = AccessTools.Field(e.GetType(), "overrideFullName");
                        field?.SetValue(e, bossName);
                    }
                }
                else
                {
                    Plugin.LogError($"EnemyIdentifier not found in {obj.name}");
                }
            }
        }
    }

    public void addId(string id) =>
        affectedEnemiesIds.Add(id);
}