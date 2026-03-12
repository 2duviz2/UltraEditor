namespace UltraEditor.Patches;

using HarmonyLib;
using System.Collections;
using UltraEditor.Classes;

[HarmonyPatch]
public static class SceneHelperPatch
{
    /// <summary> Redirect SceneHelper.LoadScene("UltraEditor"); to use our scene loader so restart mission and others work :3 </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(SceneHelper), "LoadSceneCoroutine")]
    public static bool LoadScenePatch(ref IEnumerator __result, string sceneName)
    {
        if (sceneName.StartsWith(EditorManager.EditorSceneName))
        {
            __result = EmptySceneLoader.LoadLevelAsync();
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(GetMissionName), "GetMissionNumberOnly")]
    public static bool FixMissionNum(ref string __result)
    {
        if (SceneHelper.CurrentScene.StartsWith(EditorManager.EditorSceneName))
        {
            __result = "C";
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(GetMissionName), "GetMissionNameOnly")]
    public static bool FixMissionNameOnly(ref string __result)
    {
        if (SceneHelper.CurrentScene.StartsWith(EditorManager.EditorSceneName))
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(GetMissionName), "GetMission")]
    public static bool FixMissionName(ref string __result)
    {
        if (SceneHelper.CurrentScene.StartsWith(EditorManager.EditorSceneName))
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }
}