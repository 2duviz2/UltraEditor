using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    public class LevelDownloader : MonoBehaviour
    {
        public string levelUrl;
        public string levelName;
        public string levelAuthor;
        public string pTime;
        public string pKills;
        public string pStyle;
        public string color;

        public TMP_Text levelNameText;
        public TMP_Text levelAuthorText;
        public TMP_Text levelSizeText;
        public Image levelBackground;

        public void SetTexts()
        {
            levelNameText.text = levelName;
            levelAuthorText.text = levelAuthor;
            levelSizeText.text = "Play";
            Vector3 cc = EditorManager.ParseVector3(color);
            Color c = new Color(cc.x / 255f, cc.y / 255f, cc.z / 255f);
            levelBackground.color = c;
            levelNameText.color = c;
        }

        public void Download()
        {
            if (levelUrl != "")
            {
                EmptySceneLoader.Field<GameObject>(SceneHelper.Instance, "loadingBlocker").SetActive(true);
                StartCoroutine(FetchLevels.GetStringFromUrl(levelUrl, str =>
                {
                    if (str != null)
                    {
                        EmptySceneLoader.forceEditor = false;
                        EmptySceneLoader.forceSave = "?";
                        EmptySceneLoader.forceSaveData = str;
                        EmptySceneLoader.forceLevelName = levelName;
                        EmptySceneLoader.pTime = pTime;
                        EmptySceneLoader.pKills = pKills;
                        EmptySceneLoader.pStyle = pStyle;
                        EditorManager.canOpenEditor = false;
                        EmptySceneLoader.Instance.LoadLevel();
                    }
                    else
                    {
                        EmptySceneLoader.Field<GameObject>(SceneHelper.Instance, "loadingBlocker").SetActive(false);
                    }
                }));
            }
        }
    }
}
