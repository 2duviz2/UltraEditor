namespace UltrakillStupid.Patches;

using HarmonyLib;
using UltraEditor.Classes;

[HarmonyPatch(typeof(LeaderboardController))]
[HarmonyPatch("SubmitLevelScore")]
internal class LeaderboardControllerPatch
{
    public static bool Prefix(string levelName, int difficulty, float seconds, int kills, int style, int restartCount, bool pRank = false)
    {
        return EditorManager.Instance == null;
    }
}