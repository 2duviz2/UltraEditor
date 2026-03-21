namespace UltraEditor.Patches;

using HarmonyLib;
using UltraEditor.Classes;

[HarmonyPatch(typeof(LeaderboardController), "SubmitLevelScore")]
public static class LeaderboardControllerPatch
{
    public static bool Prefix() =>
        EditorManager.Instance == null;
}