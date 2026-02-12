namespace UltraEditor.Patches;

using HarmonyLib;
using UltraEditor.Classes;

[HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Pause))]
public class RemovePauseMeun
{
    public static bool Prefix() =>
        !EditorManager.Instance?.editorOpen ?? false;
}