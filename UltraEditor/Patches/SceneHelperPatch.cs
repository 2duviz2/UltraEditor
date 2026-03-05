namespace UltrakillStupid.Patches;

using HarmonyLib;
using UltraEditor;
using UltraEditor.Classes;
using UnityEngine;

[HarmonyPatch]
public static class SceneHelperPatch
{
    /// <summary> Reload the empty scene when you restart mission in it. </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(SceneHelper), nameof(SceneHelper.RestartSceneAsync))]
    public static bool RestartMissionPatch(ref Coroutine __result)
    {
        if (SceneHelper.CurrentScene.StartsWith(EditorManager.EditorSceneName))
        {
            __result = Plugin.Instance.StartCoroutine(EmptySceneLoader.Instance.LoadLevelAsync());
            return false;
        }

        return true;
    }

    [HarmonyPrefix] [HarmonyPatch(typeof(SceneHelper), nameof(SceneHelper.RestartScene))]
    public static bool RestartMissionPatchNotAsync()
    {
        if (SceneHelper.CurrentScene.StartsWith(EditorManager.EditorSceneName))
        {
            EmptySceneLoader.Instance.LoadLevel();
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