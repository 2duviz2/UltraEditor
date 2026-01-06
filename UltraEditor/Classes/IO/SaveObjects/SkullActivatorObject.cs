using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.Editor;
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
        bool started = false;
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
                            List<GameObject> aos = [..ipz.activateOnSuccess];
                            aos.Add(to);
                            ipz.activateOnSuccess = [..aos];
                            tempObjects.Add(to);
                        }
                    }
                }

                if (tempObjects.Count == 1)
                {
                    ItemPlaceZone ipz = triggerAltars[0].transform.GetChild(0).GetComponent<ItemPlaceZone>();
                    foreach (var obj in toActivate)
                    {
                        Door d = obj.GetComponent<Door>();
                        if (d != null)
                        {
                            List<Door> drs = [.. ipz.doors];
                            drs.Add(d);
                            ipz.doors = [.. drs];
                        }
                    }
                    ipz.Invoke("Awake", 0);
                    ipz.Invoke("Start", 0);
                }
            }

            bool activated = true;
            foreach (var obj in tempObjects)
                if (!obj.activeSelf) activated = false;

            if (activated != state || !started)
            {
                TriggerActivator(activated);
                state = activated;
                started = true;
            }
        }

        public void TriggerActivator(bool a)
        {
            foreach (var obj in toActivate)
                if  (obj != null)
                    if (obj.GetComponent<Door>() == null || triggerAltars.Length > 1)
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

            toActivate = LoadingHelper.GetObjectsWithIds(toActivateIds);
            toDeactivate = LoadingHelper.GetObjectsWithIds(toDeactivateIds);
            triggerAltars = LoadingHelper.GetObjectsWithIds(triggerAltarsIds);
        }
    }
}