namespace UltrakillStupid.Patches;

using HarmonyLib;

[HarmonyPatch(typeof(StatsManager))]
[HarmonyPatch("SecretFound")]
internal class SecretFoundPatch2
{
    public static bool Prefix(int i)
    {
        return i != 100000;
    }
}