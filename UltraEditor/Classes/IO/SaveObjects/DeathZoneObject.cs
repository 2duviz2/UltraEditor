using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class DeathZoneObject : SavableObject
    {
        public bool notInstaKill;
        public int damage;
        public AffectedSubjects affected;

        public static DeathZoneObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            DeathZoneObject deathZoneObject = target.AddComponent<DeathZoneObject>();
            if (spawnedObject != null) spawnedObject.deathZoneObject = deathZoneObject;
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
}