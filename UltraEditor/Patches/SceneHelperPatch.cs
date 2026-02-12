namespace UltrakillStupid.Patches;

using HarmonyLib;
using UltraEditor.Classes;

[HarmonyPatch]
public static class SceneHelperPatch
{
    /// <summary> Reload the empty scene when you restart mission in it. </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(SceneHelper), "RestartScene")]
    public static bool RestartMissionPatch()
    {
        if (SceneHelper.CurrentScene == EditorManager.EditorSceneName)
        {
            EmptySceneLoader.Instance.LoadLevel();
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(GetMissionName), "GetMissionNumberOnly")]
    public static bool FixMissionNum(ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = "C";
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GetMissionName), "GetMissionNameOnly")]
    public static bool FixMissionNameOnly(ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(GetMissionName), "GetMission")]
    public static bool FixMissionName(ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }
}