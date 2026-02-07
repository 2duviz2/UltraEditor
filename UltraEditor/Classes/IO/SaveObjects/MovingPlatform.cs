namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

public class MovingPlatformAnimator : SavableObject
{
    public GameObject[] affectedCubes = [];
    public GameObject[] points = [];
    public List<string> pointsIds = [];
    public List<string> affectedCubesIds = [];
    public float speed = 5;
    public bool movesWithThePlayer = true;
    public platformMode mode = platformMode.EaseInOut;

    public enum platformMode
    {
        EaseIn,
        EaseOut,
        EaseInOut,
        Flat,
    }

    public static MovingPlatformAnimator Create(GameObject target)
    {
        MovingPlatformAnimator obj = target.AddComponent<MovingPlatformAnimator>();
        return obj;
    }

    public void addPointId(string id)
    {
        pointsIds.Add(id);
    }

    public void addAffectedCubeId(string id)
    {
        affectedCubesIds.Add(id);
    }

    float time = 0;
    float timeToSpawnPopup = 5;
    public override void Tick()
    {

        if (points == null || points.Length < 2 || affectedCubes == null || affectedCubes.Length == 0)
            return;

        if (transform.childCount > 0)
        {
            timeToSpawnPopup += Time.unscaledDeltaTime;
            if (timeToSpawnPopup > 5)
            {
                timeToSpawnPopup = 0;
                EditorManager.Instance.SetAlert("MovingPlatformAnimator cannot have children!");
            }
            return;
        }

        if (time == 0)
        {
            foreach (var obj in affectedCubes)
            {
                obj.tag = "Moving";
                if (obj.GetComponent<Rigidbody>() == null)
                    obj.AddComponent<Rigidbody>();
                obj.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        time += Time.deltaTime;

        int count = points.Length;
        float total = time * speed / 10;

        int aIndex = Mathf.FloorToInt(total) % count;
        int bIndex = (aIndex + 1) % count;

        float t = total - Mathf.Floor(total);

        switch (mode)
        {
            case platformMode.EaseIn:
                t = t * t;
                break;

            case platformMode.EaseOut:
                t = 1f - Mathf.Pow(1f - t, 2f);
                break;

            case platformMode.EaseInOut:
                t = t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                break;

            case platformMode.Flat:
                break;
        }

        Vector3 from = points[aIndex].transform.position;
        Vector3 to = points[bIndex].transform.position;
        Quaternion fromRot = points[aIndex].transform.rotation;
        Quaternion toRot = points[bIndex].transform.rotation;

        Vector3 targetPos = Vector3.Lerp(from, to, t);
        Quaternion targetRot = Quaternion.Lerp(fromRot, toRot, t);
        Vector3 delta = targetPos - transform.position;
        Quaternion deltaRot = targetRot * Quaternion.Inverse(transform.rotation);

        transform.position = targetPos;

        foreach (var obj in affectedCubes)
        {
            if (obj != null)
            {
                obj.transform.position += delta;
                obj.transform.rotation = deltaRot * obj.transform.rotation;
            }
        }
    }

    public void OnEnable()
    {
        time = 0;
    }

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        time = 0;
        affectedCubes = LoadingHelper.GetObjectsWithIds(affectedCubesIds);
        points = LoadingHelper.GetObjectsWithIds(pointsIds);
    }
}
