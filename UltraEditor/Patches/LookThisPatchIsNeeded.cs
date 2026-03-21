namespace UltraEditor.Patches;

using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Revolver), "Shoot")]
public static class LookThisPatchIsNeeded
{
    public static void Postfix(int shotType = 1)
    {
        CameraController.Instance.GetComponent<Camera>().fieldOfView = CameraController.Instance.defaultFov;
    }
}