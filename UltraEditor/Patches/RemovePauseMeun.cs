namespace UltraEditor.Patches;

using HarmonyLib;
using UltraEditor.Classes;

[HarmonyPatch]
public class RemovePauseMeun
{
    [HarmonyPrefix] [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
    public static bool DontFuckingExistYouFuckAssMenu() =>
        !EditorManager.Instance?.editorOpen ?? true;
}