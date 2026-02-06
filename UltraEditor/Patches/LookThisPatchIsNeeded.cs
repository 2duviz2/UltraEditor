namespace UltrakillStupid.Patches;

using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Revolver))]
[HarmonyPatch("Shoot")]
internal class LookThisPatchIsNeeded
{
    public static void Postfix(int shotType = 1)
    {
        CameraController.Instance.GetComponent<Camera>().fieldOfView = CameraController.Instance.defaultFov;
    }
}