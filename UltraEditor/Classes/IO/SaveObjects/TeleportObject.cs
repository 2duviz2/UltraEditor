namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

public class TeleportObject : SavableObject
{
    public Vector3 teleportPosition;
    public bool canBeReactivated = false;
    public bool slowdown = false;

    public static TeleportObject Create(GameObject target, SpawnedObject spawnedObject = null)
    {
        TeleportObject teleportObject = target.AddComponent<TeleportObject>();
        if (spawnedObject != null) spawnedObject.teleportObject = teleportObject;
        return teleportObject;
    }

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            NewMovement.Instance.transform.position = teleportPosition;

            if (slowdown)
                TimeController.Instance.SlowDown(0.15f);

            if (!canBeReactivated)
                Destroy(this);
        }
    }
}