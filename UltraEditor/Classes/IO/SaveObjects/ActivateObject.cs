using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class ActivateObject : SavableObject
    {
        public GameObject[] toActivate = [];
        public GameObject[] toDeactivate = [];
        public List<string> toActivateIds = [];
        public List<string> toDeactivateIds = [];
        public bool canBeReactivated = false;
        public float delay = 0;

        public static ActivateObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            ActivateObject activateObject = target.AddComponent<ActivateObject>();
            if (spawnedObject != null) spawnedObject.activateObject = activateObject;
            return activateObject;
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
        }

        public void addtoDeactivateId(string id)
        {
            toDeactivateIds.Add(id);
        }

        public override void Create()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            toActivate = LoadingHelper.GetObjectsWithIds(toActivateIds);
            toDeactivate = LoadingHelper.GetObjectsWithIds(toDeactivateIds);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Invoke("ProcessTrigger", delay);
            }
        }

        public void ProcessTrigger()
        {
            foreach (var item in toActivate)
            {
                item.SetActive(true);
            }

            foreach (var item in toDeactivate)
            {
                item.SetActive(false);
            }

            if (!canBeReactivated)
                Destroy(this);
        }
    }
}