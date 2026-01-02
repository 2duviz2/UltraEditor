namespace UltraEditor.Classes;

using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

/// <summary> Handles loading and accessing the empty scene. </summary>
public class EmptySceneLoader : MonoBehaviour
{
    /// <summary> Instance of the Loader. </summary>
    public static EmptySceneLoader Instance = null;

    /// <summary> Whether its already loaded. </summary>
    private static bool _loaded = false;

    /// <summary> Forces the editor to open as soon as the scene loads. </summary>
    public static bool forceEditor = false;

    /// <summary> Forces the editor to load a save as soon as the scene loads. </summary>
    public static string forceSave = "";

    /// <summary> Forces the editor to load a data string as soon as the scene loads, only happens if forceSave is "?". </summary>
    public static string forceSaveData = "";

    public static string pTime = "";
    public static string pKills = "";
    public static string pStyle = "";
    public static bool forceLevelCanOpenEditor = false;
    public static string forceLevelLayer = "CUSTOM LEVEL";
    public static string forceLevelImage = "null";

    /// <summary> Forces the editor to set a scene name, only happens if forceSave is "?". </summary>
    public static string forceLevelName = "";

    /// <summary> Force the loader to load. </summary>
    void Awake() => Load();

    /// <summary> Load the assetbundle containing the scene. </summary>
    public void Load()
    {
        // skip if we're already loaded
        if (_loaded || Instance != null) 
        { 
            if (Instance != null) Destroy(gameObject); 
            if (_loaded )
                return; 
        }

        DontDestroyOnLoad((Instance = this).gameObject);

        Addressables.LoadAssetAsync<GameObject>("FirstRoom").WaitForCompletion();

        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UltraEditor.Assets.emptyscene.bundle");

        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromMemoryAsync(data);
        assetRequest.completed += (_) =>
        {
            Plugin.LogInfo("Loaded Empty Scene bundle.");
            _loaded = true;
            SceneHelper.SetLoadingSubtext("");
        };
    }

    /// <summary> Loads the Empty level. </summary>
    public void LoadLevel() => StartCoroutine("LoadLevelAsync");

    /// <summary> Loads the Empty level asynchronously. </summary>
    public IEnumerator LoadLevelAsync()
    {
        Plugin.LogInfo("Loading Empty Scene.");
        Field<GameObject>(SceneHelper.Instance, "loadingBlocker").SetActive(true);
        SceneHelper.SetLoadingSubtext("Loading editor...");
        yield return null;
        
        if (!_loaded)
        {
            Plugin.LogError("Empty Scene Bundle wasn't loaded before trying to enter the scene.");
            Load();

            // wait til its loaded
            while (!_loaded) yield return null;
        }

        if (SceneHelper.CurrentScene != "UltraEditor") Property(typeof(SceneHelper), "LastScene", SceneHelper.CurrentScene);
        Property(typeof(SceneHelper), "CurrentScene", EditorManager.EditorSceneName);

        var sceneload = SceneManager.LoadSceneAsync("Assets/ULTRAEDITOR/Empty Editor Scene.unity");

        // wait til its loaded 
        while (!sceneload.isDone) 
        { Plugin.LogInfo("waiting for sceneload to complete"); yield return null; }

        Plugin.LogInfo("Scene loaded!");

        Field<GameObject>(SceneHelper.Instance, "loadingBlocker").SetActive(false);

        /*GameObject statsManager = new GameObject("StatsManager", typeof(StatsManager));
        GameObject levelInfo = new GameObject("LevelInfo", typeof(StockMapInfo));

        GameObject firstRoom = Instantiate(Plugin.Ass<GameObject>("FirstRoom"));*/

        if (forceEditor)
        {
            EditorManager.canOpenEditor = false;
            forceLevelCanOpenEditor = false;
            while (!NewMovement.Instance.activated && SceneHelper.PendingScene == null) { yield return null; }
            OpenEditor();
            forceLevelLayer = "CUSTOM LEVEL";
        }

        else if (forceSave != "")
        {
            EditorManager.canOpenEditor = false;
            EditorManager.Create();
            EditorManager.DeleteScene(true);
            string levelName = forceSave.Replace(".uterus", "");
            if (forceSave != "?")
            {
                EditorManager.Instance.LoadShit(forceSave);
                forceLevelCanOpenEditor = false;
                forceLevelLayer = "CUSTOM LEVEL";
            }
            else
            {
                EditorManager.Instance.LoadSceneJson(forceSaveData);
                levelName = forceLevelName;

                int pt = int.Parse(pTime);
                int pk = int.Parse(pKills);
                int ps = int.Parse(pStyle);

                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.timeRanks[3 - i] = (int)(pt * ((i*0.25f) + 1f));
                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.killRanks[3 - i] = pk - (i * 5);
                for (int i = 0; i < 4; i++)
                    StatsManager.Instance.styleRanks[3 - i] = ps - (i * 1000);

                StockMapInfo.Instance.assets.LargeImage = forceLevelImage;
            }
            List<GameObject> secrets = [];
            int ind = 0;
            foreach (var secret in FindObjectsOfType<Bonus>(true))
            {
                secret.secretNumber = ind;
                secrets.Add(secret.gameObject);
                ind++;
            }
            LevelInfoObject lio = FindObjectOfType<LevelInfoObject>();
            StatsManager.Instance.secretObjects = secrets.ToArray();
            EditorManager.Instance.CreateUI();
            StockMapInfo.Instance.levelName = levelName.ToUpper();
            StockMapInfo.Instance.layerName = StockMapInfo.Instance.layerName.Replace("EMPTY", forceLevelLayer);
            StockMapInfo.Instance.assets.LargeText = levelName.ToUpper();
            if (lio != null)
            {
                StockMapInfo.Instance.tipOfTheDay = new ScriptableObjects.TipOfTheDay() { tip = lio.tipOfTheDay };
                StockMapInfo.Instance.layerName = lio.levelLayer;
            }
            else
                StockMapInfo.Instance.tipOfTheDay = new ScriptableObjects.TipOfTheDay() { tip = "Welcome!" };
            ShopZone[] sz = FindObjectsOfType<ShopZone>(true);
            foreach (var s in sz)
            {
                if (s.tipOfTheDay != null)
                    s.tipOfTheDay.text = StockMapInfo.Instance.tipOfTheDay.tip;
            }
            StockMapInfo.Instance.assets.LargeText = levelName.ToUpper();
            LevelNamePopup.Instance.Invoke("Start", 0);
            DiscordController.Instance.FetchSceneActivity(SceneHelper.CurrentScene);
            SteamController.Instance.FetchSceneActivity(SceneHelper.CurrentScene);
        }

        ChallengeManager.Instance.challengePanel.transform.parent.Find("ChallengeText").GetComponent<TMP_Text>().text = "(NO CHALLENGE)";
        MusicManager.Instance.battleTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.cleanTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.bossTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.GetComponent<AudioSource>().outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;

        yield break;
    }

    public void OpenEditor()
    {
        if (SceneHelper.PendingScene != null) return;

        EditorManager.canOpenEditor = true;
        EditorManager.Create();
    }

    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneHelper), "RestartScene")]
        public static bool RestartMissionPatch()
        {
            if (SceneHelper.CurrentScene == EditorManager.EditorSceneName)
            {
                Instance.LoadLevel();
                return false;
            }

            return true;
        }
    }

    #region tools
    /// <summary> Gets or Sets a field regardless if its private or not. </summary>
    /// <param name="InstanceOrType">Instance or type of the field. (use an instance or non-static fields, and a type for static fields.)</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">Value to set the field to.</param>
    public static T Field<T>(object InstanceOrType, string fieldName, object value = null) =>
        (T)Field(InstanceOrType, fieldName, value);

    /// <summary> Gets or Sets a property regardless if its private or not. </summary>
    /// <param name="InstanceOrType">Instance or type of the property. (use an instance or non-static properties, and a type for static properties.)</param>
    /// <param name="propName">The name of the property.</param>
    /// <param name="value">Value to set the property to.</param>
    public static T Property<T>(object InstanceOrType, string propName, object value = null) =>
        (T)Property(InstanceOrType, propName, value);

    /// <summary> Tools.Field cache. </summary>
    private static Dictionary<(object, string), FieldInfo> Field_Cache = [];

    /// <summary> Gets or Sets a field regardless if its private or not. </summary>
    /// <param name="InstanceOrType">Instance or type of the field. (use an instance or non-static fields, and a type for static fields.)</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">Value to set the field to.</param>
    public static object Field(object InstanceOrType, string fieldName, object value = null)
    {
        Type type = InstanceOrType is Type _type ? _type : InstanceOrType.GetType();
        var key = (InstanceOrType, fieldName);
        if (!Field_Cache.TryGetValue(key, out var field))
            Field_Cache[key] = field = AccessTools.Field(type, fieldName);

        object obj = field?.IsStatic ?? false ? null : InstanceOrType;

        if (value != null)
            field?.SetValue(obj, value);

        return field?.GetValue(obj);
    }

    /// <summary> Tools.Property cache. </summary>
    private static Dictionary<(object, string), PropertyInfo> Property_Cache = [];

    /// <summary> Gets or Sets a property regardless if its private or not. </summary>
    /// <param name="InstanceOrType">Instance or type of the property. (use an instance or non-static properties, and a type for static properties.)</param>
    /// <param name="propName">The name of the property.</param>
    /// <param name="value">Value to set the property to.</param>
    public static object Property(object InstanceOrType, string propName, object value = null)
    {
        Type type = InstanceOrType is Type _type ? _type : InstanceOrType.GetType();
        object obj = InstanceOrType is Type ? null : InstanceOrType;

        var key = (InstanceOrType, propName);
        if (!Property_Cache.TryGetValue(key, out var property))
            Property_Cache[key] = property = AccessTools.Property(type, propName);

        if (value != null) // Set
            property?.GetSetMethod(true)?.Invoke(obj, [value]);

        return property?.GetGetMethod(true)?.Invoke(obj, null);
    }

    /// <summary> Tools.InvokeMethod cache. </summary>
    private static Dictionary<(object, string), MethodBase> InvokeMethod_Cache = [];

    /// <summary> Invokes a method regardless if its private or not. </summary>
    /// <param name="InstanceOrType">Instance or type of the method to invoke. (use an instance or non-static methods, and a type for static methods.)</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="args">Arguements to be given to the method.</param>
    public static void InvokeMethod(object InstanceOrType, string methodName, params object[] args)
    {
        Type type = InstanceOrType is Type _type ? _type : InstanceOrType.GetType();
        var key = (InstanceOrType, methodName);
        if (!InvokeMethod_Cache.TryGetValue(key, out var method))
            InvokeMethod_Cache[key] = method = AccessTools.Method(type, methodName);

        method.Invoke(InstanceOrType, args);
    }
    #endregion
}
