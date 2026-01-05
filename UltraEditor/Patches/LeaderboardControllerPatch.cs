using HarmonyLib;
using Logic;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraEditor.Classes;
using UnityEngine;

namespace UltrakillStupid.Patches
{
    [HarmonyPatch(typeof(LeaderboardController))]
    [HarmonyPatch("SubmitLevelScore")]
    internal class LeaderboardControllerPatch
    {
        public static bool Prefix(string levelName, int difficulty, float seconds, int kills, int style, int restartCount, bool pRank = false)
        {
            return EditorManager.Instance == null;
        }
    }
}