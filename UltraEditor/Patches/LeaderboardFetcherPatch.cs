using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UltraEditor;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Patches
{
    [HarmonyPatch(typeof(LevelSelectLeaderboard))]
    internal class LeaderboardFetcherPatch
    {
        [HarmonyPatch("Fetch")]
        [HarmonyPrefix]
        private static bool Fetch_Prefix(LevelSelectLeaderboard __instance, string levelName, ref IEnumerator __result)
        {
            __result = CustomFetch(__instance, levelName);
            return false;
        }

        private static IEnumerator CustomFetch(LevelSelectLeaderboard instance, string levelName)
        {
            var templateUsername = Traverse.Create(instance).Field("templateUsername").GetValue<TMP_Text>();
            var templateTime = Traverse.Create(instance).Field("templateTime").GetValue<TMP_Text>();
            var templateDifficulty = Traverse.Create(instance).Field("templateDifficulty").GetValue<TMP_Text>();
            var template = Traverse.Create(instance).Field("template").GetValue<GameObject>();
            var container = Traverse.Create(instance).Field("container").GetValue<Transform>();
            var loadingPanel = Traverse.Create(instance).Field("loadingPanel").GetValue<GameObject>();
            var noItemsPanel = Traverse.Create(instance).Field("noItemsPanel").GetValue<GameObject>();
            var scrollRectContainer = Traverse.Create(instance).Field("scrollRectContainer").GetValue<GameObject>();
            var pRankSelected = Traverse.Create(instance).Field("pRankSelected").GetValue<bool>();

            if (string.IsNullOrEmpty(levelName))
                yield break;

            Task<LeaderboardEntry[]> entryTask =
                MonoSingleton<LeaderboardController>.Instance.GetLevelScores(levelName, pRankSelected);

            while (!entryTask.IsCompleted)
                yield return null;

            if (entryTask.Result == null)
                yield break;

            LeaderboardEntry[] result = entryTask.Result;

            foreach (LeaderboardEntry leaderboardEntry in result)
            {
                TMP_Text tmp_Text = templateUsername;
                Friend user = leaderboardEntry.User;
                tmp_Text.text = user.Name;

                int score = leaderboardEntry.Score;
                int minutes = score / 60000;
                float seconds = (float)(score - minutes * 60000) / 1000f;
                int kills = leaderboardEntry.Details[1];
                int style = leaderboardEntry.Details[2];
                int restartCount = leaderboardEntry.Details[3];
                templateTime.text = $"{string.Format("{0}:{1:00.000}", minutes, seconds)}\nK={kills} R={restartCount}";

                int? difficultyIndex = null;
                if (leaderboardEntry.Details.Length != 0)
                {
                    difficultyIndex = leaderboardEntry.Details[0];
                }

                if (difficultyIndex != null &&
                    difficultyIndex.Value < LeaderboardProperties.Difficulties.Length)
                {
                    templateDifficulty.text = LeaderboardProperties.Difficulties[difficultyIndex.Value].ToUpper();

                    GameObject gameObject = Object.Instantiate(template, container);
                    gameObject.SetActive(true);

                    SteamController.FetchAvatar(gameObject.GetComponentInChildren<RawImage>(), leaderboardEntry.User);
                }
                else
                {
                    templateDifficulty.text = "UNKNOWN";
                }

                if (leaderboardEntry.Details.Length > 1)
                {
                    //Plugin.LogInfo($"Player {user.Name} had {kills} kills.");
                }

                //Plugin.LogInfo($"Fetched leaderboard entry: {levelName} -  {user.Name} - {templateTime.text} - {templateDifficulty.text}/{difficultyIndex}");
            }

            if (result.Length == 0)
            {
                noItemsPanel.SetActive(true);
            }

            loadingPanel.SetActive(false);
            container.gameObject.SetActive(true);
            scrollRectContainer.SetActive(true);

            yield break;
        }
    }
}