using HarmonyLib;

namespace UltrakillStupid.Patches
{
    [HarmonyPatch(typeof(GameProgressSaver))]
    [HarmonyPatch("SecretFound")]
    internal class SecretFoundPatch
    {
        public static bool Prefix(int secretNum)
        {
            return secretNum != 100000;
        }
    }
}