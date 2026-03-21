namespace UltraEditor.Patches;

using HarmonyLib;

[HarmonyPatch(typeof(GameProgressSaver), "SecretFound")]
public class GmameProggressSaverPatch
{
    public static bool Prefix(int secretNum)
    {
        return true;
    }
}