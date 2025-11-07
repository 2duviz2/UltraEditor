using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    internal class DeathZoneObject : SavableObject
    {
        public bool notInstaKill;
        public int damage;
        public AffectedSubjects affected;

        public static DeathZoneObject Create(GameObject target)
        {
            DeathZoneObject obj = target.AddComponent<DeathZoneObject>();
            return obj;
        }

        public void createDeathzone()
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
