namespace UltrakillStupid.Patches;

using HarmonyLib;

[HarmonyPatch(typeof(GameProgressSaver))]
[HarmonyPatch("SecretFound")]
internal class GmameProggressSaverPatch
{
    public static bool Prefix(int secretNum)
    {
        return true;
    }
}