namespace UltraEditor.Classes.Canvas;

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FetchLevels : MonoBehaviour
{
    public GameObject template;
    public Transform container;

    string url = "https://duviz.xyz/api/ultraeditor/fetchlevels";
    string getLevelUrl = "https://duviz.xyz/api/ultraeditor/getlevel/";
    string downloadLevelUrl = "https://duviz.xyz/api/ultraeditor/downloadlevel/";
    string getImageUrl = "https://duviz.xyz/api/ultraeditor/getimg/";

    [Serializable]
    public class LevelData
    {
        public string author;
        public string data;
        public int id;
        public string color;
        public string image;
        public string name;
        public string ptime;
        public string pkills;
        public string pstyle;
        public string rank;
        public string layer;
        public int canOpenEditor;
    }

    public void Start()
    {
        ImageGetter._loaded = true;

        StartCoroutine(FetchLevels.GetStringFromUrl(url, str =>
        {
            if (str != null)
            {
                int parsedNum = 0;
                int.TryParse(str, out parsedNum);

                if (parsedNum == 0) return;

                StartCoroutine(LoadLevels(parsedNum));
            }
        }));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Destroy(gameObject);
    }

    public IEnumerator LoadLevels(int parsedNum)
    {
        bool _loaded = true;
        for (int i = 0; i < parsedNum; i++)
        {
            int index = i;
            _loaded = false;
            StartCoroutine(FetchLevels.GetStringFromUrl($"{getLevelUrl}{i}", str2 =>
            {
                if (str2 != null)
                {
                    LevelData levelData = JsonUtility.FromJson<LevelData>(str2);

                    GameObject lv = Instantiate(template, container);
                    lv.GetComponent<ImageGetter>().imageUrl = $"{getImageUrl}{levelData.id}";
                    lv.GetComponent<ImageGetter>().SetImg();
                    lv.GetComponent<LevelDownloader>().levelUrl = $"{downloadLevelUrl}{levelData.id}";
                    lv.GetComponent<LevelDownloader>().levelName = levelData.name;
                    lv.GetComponent<LevelDownloader>().levelAuthor = levelData.author;
                    lv.GetComponent<LevelDownloader>().pTime = levelData.ptime;
                    lv.GetComponent<LevelDownloader>().pKills = levelData.pkills;
                    lv.GetComponent<LevelDownloader>().pStyle = levelData.pstyle;
                    lv.GetComponent<LevelDownloader>().color = levelData.color;
                    lv.GetComponent<LevelDownloader>().levelLayer = levelData.layer;
                    lv.GetComponent<LevelDownloader>().canOpenEditor = levelData.canOpenEditor == 1;
                    lv.GetComponent<LevelDownloader>().levelImageUrl = $"{getImageUrl}{levelData.id}";
                    lv.GetComponent<LevelDownloader>().SetTexts();
                }
                _loaded = true;
            }));
            while (!_loaded) yield return null;
        }
    }

    public static IEnumerator GetStringFromUrl(string url, System.Action<string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load string: " + www.error);
                callback?.Invoke(null);
            }
            else
            {
                callback?.Invoke(www.downloadHandler.text);
            }
        }
    }
}
