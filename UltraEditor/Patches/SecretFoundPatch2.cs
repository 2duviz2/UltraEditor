namespace UltraEditor.Patches;

using HarmonyLib;

[HarmonyPatch(typeof(StatsManager), "SecretFound")]
public static class SecretFoundPatch2
{
    public static bool Prefix(int i) =>
        i != 100000;
}