namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

public class DeathZoneObject : SavableObject
{
    public bool notInstaKill;
    public int damage;
    public AffectedSubjects affected;

    public static DeathZoneObject Create(GameObject target)
    {
        DeathZoneObject deathZoneObject = target.AddComponent<DeathZoneObject>();
        return deathZoneObject;
    }

    public override void Create()
    {
        DeathZone deathZone = gameObject.AddComponent<DeathZone>();

        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;

        deathZone.notInstakill = notInstaKill;
        deathZone.damage = damage;
        deathZone.affected = affected;
    }
}