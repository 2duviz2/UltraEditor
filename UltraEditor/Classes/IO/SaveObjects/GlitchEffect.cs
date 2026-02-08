namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using UltraEditor.Classes.TempScripts;
using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Spawns clones of everything in <b>Dummy objects</b> within <b>Range</b> in the position of <b>this component</b>.")]
public class GlitchEffect : SavableObject
{
    [EditorVar("Dummy objects")]
    public GameObject[] dummyObjects = [];

    public List<string> dummyObjectsIds = [];

    [EditorVar("Range")]
    public float range = 5;

    [EditorVar("Start range")]
    public float startRange = 0;

    [EditorVar("Copy lifespan")]
    public float copyLifespan = .5f;

    [EditorVar("Copy amount")]
    public float copyAmount = 5;

    public static GlitchEffect Create(GameObject target) =>
        target.AddComponent<GlitchEffect>();

    float timePassed = 1000;

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        dummyObjects = LoadingHelper.GetObjectsWithIds(dummyObjectsIds);
    }

    public override void Tick()
    {
        if (Time.timeScale == 0) return;
        if (dummyObjects.Length == 0) return;
        timePassed += Time.deltaTime;
        if (timePassed > 1f / copyAmount)
        {
            timePassed = 0;
            CreateCopy();
        }
    }

    public void CreateCopy()
    {
        Vector3 dir = Random.onUnitSphere;
        float dist = Random.Range(startRange, range);
        Vector3 pos = transform.position + dir * dist;

        GameObject cObj = dummyObjects[Random.Range(0, dummyObjects.Length)];
        if (cObj == null) return;

        GameObject obj = Instantiate(cObj, pos, cObj.transform.rotation);

        obj.SetActive(true);
        obj.AddComponent<DeleteOnEditor>();
        obj.AddComponent<DeleteAfterTime>().time = copyLifespan;
    }

    public void addId(string id) =>
        dummyObjectsIds.Add(id);
}