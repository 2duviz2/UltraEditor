using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class SkullActivatorObject : SavableObject
    {
        public enum skullType
        {
            blueSkull,
            redSkull,
            torch
        }

        public Dictionary<skullType, ItemType> itemTypes = new()
        {
            [skullType.blueSkull] = ItemType.SkullBlue,
            [skullType.redSkull] = ItemType.SkullRed,
            [skullType.torch] = ItemType.Torch,
        };

        public skullType acceptedItemType;
        public GameObject[] triggerAltars = [];
        public GameObject[] toActivate = [];
        public GameObject[] toDeactivate = [];
        public List<string> toActivateIds = [];
        public List<string> toDeactivateIds = [];
        public List<string> triggerAltarsIds = [];

        public static SkullActivatorObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            SkullActivatorObject obj = target.AddComponent<SkullActivatorObject>();
            spawnedObject?.skullActivatorObject = obj;
            return obj;
        }

        public void addToActivateId(string id)
        {
            toActivateIds.Add(id);
        }

        public void addToDeactivateId(string id)
        {
            toDeactivateIds.Add(id);
        }

        public void addTriggerAltarId(string id)
        {
            triggerAltarsIds.Add(id);
        }

        bool state = false; // off / on
        bool tried = false;
        List<GameObject> tempObjects = [];
        public void Tick()
        {
            if (Time.timeScale == 0) return;
            if (tempObjects.Count == 0 && !tried)
            {
                tried = true;
                foreach (var obj in triggerAltars)
                {
                    if (obj.transform.childCount > 0)
                    {
                        Transform cube = obj.transform.GetChild(0);
                        ItemPlaceZone ipz = cube.GetComponent<ItemPlaceZone>();
                        if (ipz != null)
                        {
                            GameObject to = new GameObject($"{obj.name} listener");
                            to.SetActive(false);
                            List<GameObject> aos = ipz.activateOnSuccess.ToList();
                            aos.Add(to);
                            ipz.activateOnSuccess = aos.ToArray();
                            tempObjects.Add(to);
                        }
                    }
                }
            }

            bool activated = true;
            foreach (var obj in tempObjects)
            {
                if (!obj.activeSelf) activated = false;
            }

            if (activated != state)
            {
                TriggerActivator(activated);
                state = activated;
            }
        }

        public void TriggerActivator(bool a)
        {
            foreach (var obj in toActivate)
                if  (obj != null)
                    obj.SetActive(a);
            foreach (var obj in toDeactivate)
                if (obj != null)
                    obj.SetActive(!a);
        }

        public void createActivator()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            toActivate = [];
            toDeactivate = [];
            triggerAltars = [];

            foreach (var e in toActivateIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> objs = toActivate.ToList();
                        objs.Add(obj.gameObject);
                        toActivate = objs.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> objs = toActivate.ToList();
                            objs.Add(obj.gameObject);
                            toActivate = objs.ToArray();
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
                        List<GameObject> objs = toDeactivate.ToList();
                        objs.Add(obj.gameObject);
                        toDeactivate = objs.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> objs = toDeactivate.ToList();
                            objs.Add(obj.gameObject);
                            toDeactivate = objs.ToArray();
                            found = true;
                            break;
                        }
                    }
            }
            foreach (var e in triggerAltarsIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> objs = triggerAltars.ToList();
                        objs.Add(obj.gameObject);
                        triggerAltars = objs.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> objs = triggerAltars.ToList();
                            objs.Add(obj.gameObject);
                            triggerAltars = objs.ToArray();
                            found = true;
                            break;
                        }
                    }
            }
        }
    }
}