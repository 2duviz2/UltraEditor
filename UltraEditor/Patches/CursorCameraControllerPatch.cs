namespace UltraEditor.Patches;

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Debug = UnityEngine.Debug;

[HarmonyPatch]
public static class CursorCameraControllerPatch
{
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.LateUpdate))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixCursor(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo get_isDebugBuild = AccessTools.Method(typeof(Debug), "get_isDebugBuild"); // Debug.isDebugBuild
        foreach (CodeInstruction instr in instructions)
        {
            if (instr.Calls(get_isDebugBuild)) // if (X && [isDebugBuild])
                yield return new CodeInstruction(OpCodes.Ldc_I4_0); // [False]
            else
                yield return instr; // Just continue idk
        }
    }
}