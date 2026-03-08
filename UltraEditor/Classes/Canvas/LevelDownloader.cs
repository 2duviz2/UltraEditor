namespace UltraEditor.Classes.Canvas;

using TMPro;
using UltraEditor.Classes.Editor;
using UnityEngine;
using UnityEngine.UI;

public class LevelDownloader : MonoBehaviour
{
    public string levelUrl;
    public string levelName;
    public string levelAuthor;
    public string pTime;
    public string pKills;
    public string pStyle;
    public string color;
    public string levelLayer;
    public string levelImageUrl;
    public string levelGUID;
    public bool canOpenEditor;

    public TMP_Text levelNameText;
    public TMP_Text levelAuthorText;
    public TMP_Text levelSizeText;
    public Image levelBackground;

    public void SetTexts()
    {
        levelNameText.text = levelName;
        levelAuthorText.text = levelAuthor;
        levelSizeText.text = "Play";
        Vector3 cc = ParseHelper.ParseVector3(color);
        Color c = new(cc.x / 255f, cc.y / 255f, cc.z / 255f);
        levelBackground.color = c;
        levelNameText.color = c;
    }

    public void Download()
    {
        if (levelUrl == "")
            return;

        SceneHelper.Instance.loadingBlocker.SetActive(true);
        SceneHelper.SetLoadingSubtext("Downloading level...");
        StartCoroutine(FetchLevels.GetStringFromUrl(levelUrl, str =>
        {
            if (str == null)
            {
                SceneHelper.Instance.loadingBlocker.SetActive(false);
                SceneHelper.SetLoadingSubtext("");
                return;
            }

            EmptySceneLoader.forceEditor = false;
            EmptySceneLoader.forceSave = "?";
            EmptySceneLoader.forceSaveData = str;
            EmptySceneLoader.forceLevelName = levelName;
            EmptySceneLoader.forceLevelLayer = levelLayer;
            EmptySceneLoader.forceLevelCanOpenEditor = canOpenEditor;
            EmptySceneLoader.forceLevelGUID = levelGUID;
            EmptySceneLoader.pTime = pTime;
            EmptySceneLoader.pKills = pKills;
            EmptySceneLoader.pStyle = pStyle;
            EmptySceneLoader.forceLevelImage = levelImageUrl;
            EditorManager.canOpenEditor = false;
            EmptySceneLoader.LoadLevel();
        }));
    }
}
