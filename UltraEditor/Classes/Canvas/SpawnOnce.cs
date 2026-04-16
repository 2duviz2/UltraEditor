namespace UltraEditor.Classes.Canvas;

using System.Collections.Generic;
using UltraEditor.Libraries;
using UnityEngine;

public class SpawnOnce : MonoBehaviour
{
    public GameObject objectToActivate;
    public string nameOfSpawn = "default";

    public static List<string> spawned = [];

    public void Start()
    {
        if (Preferences.GetInt("WelcomePopup", 1) == 0) return;
        if (!spawned.Contains(nameOfSpawn))
        {
            spawned.Add(nameOfSpawn);
            objectToActivate.SetActive(true);
        }
        else
            objectToActivate.SetActive(false);
    }
}
