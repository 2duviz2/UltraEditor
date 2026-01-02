using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class SpawnOnce : MonoBehaviour
    {
        public GameObject objectToActivate;
        public string nameOfSpawn = "default";

        public static List<string> spawned = [];

        public void Start()
        {
            if (!spawned.Contains(nameOfSpawn))
            {
                spawned.Add(nameOfSpawn);
                objectToActivate.SetActive(true);
            }
            else
                objectToActivate.SetActive(false);
        }
    }
}
