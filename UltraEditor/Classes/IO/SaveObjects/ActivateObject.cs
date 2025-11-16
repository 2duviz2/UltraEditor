using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    internal class ActivateObject : SavableObject
    {
        public GameObject[] toActivate = [];
        public GameObject[] toDeactivate = [];
        public List<string> toActivateIds = [];
        public List<string> toDeactivateIds = [];
        public bool canBeReactivated = false;

        public static ActivateObject Create(GameObject target)
        {
            ActivateObject obj = target.AddComponent<ActivateObject>();
            return obj;
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
        }

        public void addtoDeactivateId(string id)
        {
            toDeactivateIds.Add(id);
        }

        public void createActivator()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            foreach (var e in toActivateIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> activate = (toActivate ?? []).ToList();
                        activate.Add(obj.gameObject);
                        toActivate = activate.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> activate = (toActivate ?? []).ToList();
                            activate.Add(obj.gameObject);
                            toActivate = activate.ToArray();
                            found = true;
                            break;
                        }
                    }
            }

            foreach (var e in toDeactivateIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> deactivate = (toDeactivate ?? []).ToList();
                        deactivate.Add(obj.gameObject);
                        toDeactivate = deactivate.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> deactivate = (toDeactivate ?? []).ToList();
                            deactivate.Add(obj.gameObject);
                            toDeactivate = deactivate.ToArray();
                            found = true;
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

                foreach (var item in toDeactivate)
                {
                    item.SetActive(false);
                }

                if (!canBeReactivated)
                    Destroy(this);
            }
        }
    }
}