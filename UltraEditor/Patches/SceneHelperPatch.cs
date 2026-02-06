namespace UltrakillStupid.Patches;

using HarmonyLib;

[HarmonyPatch(typeof(GetMissionName))]
[HarmonyPatch("GetMissionNameOnly")]
internal class GetMissionNamePatch
{
    public static bool Prefix(int missionNum, ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(GetMissionName))]
[HarmonyPatch("GetMissionNumberOnly")]
internal class GetMissionNamePatch2
{
    public static bool Prefix(int missionNum, ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = "C";
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(GetMissionName))]
[HarmonyPatch("GetMission")]
internal class GetMissionNamePatch3
{
    public static bool Prefix(int missionNum, ref string __result)
    {
        if (SceneHelper.CurrentScene == "UltraEditor")
        {
            __result = MapInfoBase.Instance.levelName;
            return false;
        }

        return true;
    }
}