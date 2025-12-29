using HarmonyLib;

namespace UltrakillStupid.Patches
{
    [HarmonyPatch(typeof(GameProgressSaver))]
    [HarmonyPatch("SecretFound")]
    internal class GmameProggressSaverPatch
    {
        public static bool Prefix(int secretNum)
        {
            //return secretNum != 100000;
            return true;
        }
    }
}