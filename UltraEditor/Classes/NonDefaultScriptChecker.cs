using System;
using UltraEditor;
using UnityEngine;

public class NonDefaultScriptChecker : MonoBehaviour
{
    public static void CheckScripts()
    {
        MonoBehaviour[] allMBs = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in allMBs)
        {
            Type t = mb.GetType();
            string assemblyName = t.Assembly.GetName().Name;

            if (assemblyName != "Assembly-CSharp" &&
                !assemblyName.StartsWith("UnityEngine") &&
                !assemblyName.StartsWith("Unity") &&
                !assemblyName.StartsWith("UnityEditor"))
            {
                Plugin.LogError($"Non-default script detected: {t.FullName} in assembly {assemblyName} on GameObject '{mb.gameObject.name}'");
            }
        }

        Plugin.LogInfo("Script scan complete");
    }
}