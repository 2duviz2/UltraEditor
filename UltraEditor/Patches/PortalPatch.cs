namespace UltraEditor.Patches;

using HarmonyLib;
using System.Linq;
using ULTRAKILL.Portal;

[HarmonyPatch]
public static class PortalPatch
{
    /// <summary> Make the game shut the fuck up about null portals. </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(PortalManagerV2), "Update")]
    public static void ShutTheFuckUp(PortalManagerV2 __instance) =>
        __instance.portalComponents = [.. __instance.portalComponents.Where(p => p)];
}
