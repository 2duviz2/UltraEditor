namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

public class SavableObject : MonoBehaviour
{
    public string Name;
    public Vector3 Position;
    public Vector3 EulerAngles;
    public Vector3 Scale;

    public void Update()
    {
        Name = gameObject.name;
        Position = transform.position;
        EulerAngles = transform.eulerAngles;
        Scale = transform.lossyScale;

        Tick();
    }

    public virtual void Tick() { }
    public virtual void Create() { }

    public void DisableNavmesh()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        DisableCollision();
    }

    public void DisableCollision() => gameObject.GetComponent<Collider>().isTrigger = true;
}