using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    internal class ActivateObject : SavableObject
    {
        public GameObject[] toActivate = [];
        public List<string> toActivateIds = [];

        public static ActivateObject Create(GameObject target)
        {
            ActivateObject obj = target.AddComponent<ActivateObject>();
            return obj;
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
        }

        public void createActivator()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            foreach (var e in toActivateIds)
            {
                foreach (var obj in GameObject.FindObjectsOfType<SpawnedObject>(true))
                {
                    if (e == obj.ID)
                    {
                        List<GameObject> activate = (toActivate ?? []).ToList();
                        activate.Add(obj.gameObject);
                        toActivate = activate.ToArray();
                        break;
                    }
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                foreach (var item in toActivate)
                {
                    item.SetActive(true);
                }

                Destroy(this);
            }
        }
    }
}