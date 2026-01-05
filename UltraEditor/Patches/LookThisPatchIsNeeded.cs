using HarmonyLib;
using UnityEngine;

namespace UltrakillStupid.Patches
{
    [HarmonyPatch(typeof(Revolver))]
    [HarmonyPatch("Shoot")]
    internal class LookThisPatchIsNeeded
    {
        public static void Postfix(int shotType = 1)
        {
            CameraController.Instance.GetComponent<Camera>().fieldOfView = CameraController.Instance.defaultFov;
        }
    }
}