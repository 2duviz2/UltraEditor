namespace UltrakillStupid.Patches;

using HarmonyLib;
using UltraEditor.Classes;
using UnityEngine;

[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
[HarmonyPatch()]
public class RemovePauseMeun
{
    public static bool Prefix()
    {
        return EditorManager.Instance == null || !EditorManager.Instance.editorOpen;
    }
}