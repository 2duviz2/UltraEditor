namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

public class CubeTilingAnimator : SavableObject
{
    public Vector2 scrolling = Vector2.one;
    public GameObject[] affectedCubes = [];
    public List<string> affectedCubesIds = [];

    public static CubeTilingAnimator Create(GameObject target, SpawnedObject spawnedObject = null)
    {
        CubeTilingAnimator obj = target.AddComponent<CubeTilingAnimator>();
        if (spawnedObject != null) spawnedObject.cubeTilingAnimator = obj;
        return obj;
    }

    public void addId(string id)
    {
        affectedCubesIds.Add(id);
    }

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        affectedCubes = LoadingHelper.GetObjectsWithIds(affectedCubesIds);
    }

    public override void Tick()
    {
        foreach (var c in affectedCubes)
        {
            if (c == null) continue;
            MaterialChoser mc = c.GetComponent<MaterialChoser>();

            if (mc != null)
            {
                mc.offset += scrolling * Time.unscaledDeltaTime;
            }
        }
    }
}
