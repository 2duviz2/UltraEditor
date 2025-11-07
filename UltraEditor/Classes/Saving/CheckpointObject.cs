using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.Saving
{
    internal class CheckpointObject : SavableObject
    {
        public List<string> rooms = new List<string>();
        public List<string> roomsToInherit = new List<string>();

        public static CheckpointObject Create(GameObject target)
        {
            CheckpointObject obj = target.AddComponent<CheckpointObject>();
            if (obj.GetComponent<Collider>() != null)
                obj.GetComponent<Collider>().isTrigger = true;
            return obj;
        }

        public void addRoomId(string id)
        {
            rooms.Add(id);
        }

        public void addRoomToInheritId(string id)
        {
            roomsToInherit.Add(id);
        }

        public void createCheckpoint()
        {
            StartCoroutine(waitTillPlayer());
        }

        IEnumerator waitTillPlayer()
        {
            while (!NewMovement.Instance.gameObject.activeInHierarchy)
            {
                yield return new WaitForEndOfFrame();
            }
            GameObject so = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Levels/Checkpoint.prefab"), transform);
            so.transform.localPosition = Vector3.zero;
            so.transform.localEulerAngles = Vector3.zero;
            so.transform.localScale = Vector3.one;

            CheckPoint checkpoint = so.GetComponent<CheckPoint>();
            checkpoint.rooms = [];
            checkpoint.roomsToInherit = [];

            foreach (var e in rooms)
            {
                foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> rooms = (checkpoint.rooms ?? new GameObject[0]).ToList();
                        rooms.Add(obj.gameObject);
                        checkpoint.rooms = rooms.ToArray();
                        break;
                    }
                }
            }

            foreach (var e in roomsToInherit)
            {
                foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> rooms = checkpoint.roomsToInherit ?? [];
                        rooms.Add(obj.gameObject);
                        checkpoint.roomsToInherit = rooms;
                        break;
                    }
                }
            }
        }
    }
}
