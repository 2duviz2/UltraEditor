namespace UltraEditor;

using BepInEx;
using HarmonyLib;
using System;
using UltraEditor.Classes;
using UltraEditor.Classes.Editor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[BepInPlugin(GUID, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "duviz.ultrakill.ultraeditor";
    public const string Name = "UltraEditor";
    public const string Version = "0.0.9";

    public static Plugin instance;
    public plog.Logger Log;

    public static KeyCode editorOpenKey = KeyCode.F1;
    public static KeyCode selectCursorKey = KeyCode.F2;
    public static KeyCode selectMoveKey = KeyCode.F3;
    public static KeyCode selectScaleKey = KeyCode.F4;
    public static KeyCode selectRotationKey = KeyCode.F5;
    public static KeyCode toggleEditorCanvasKey = KeyCode.F9;
    public static KeyCode deleteObjectKey = KeyCode.Delete;
    public static KeyCode createCubeKey = KeyCode.KeypadPlus;
    public static KeyCode ctrlKey = KeyCode.LeftControl;
    public static KeyCode shiftKey = KeyCode.LeftShift;
    public static KeyCode altKey = KeyCode.LeftAlt;

    static bool seenWelcomeMessage = false;

    public static bool isToggleEnabledKeyPressed()
    {
        if (Input.GetKey(altKey) && Input.GetKey(shiftKey) && Input.GetKeyDown(KeyCode.A))
        {
            return true;
        }
        return false;
    }

    public static bool isDuplicateKeyPressed()
    {
        if (Input.GetKey(ctrlKey) && Input.GetKeyDown(KeyCode.D))
        {
            return true;
        }
        return false;
    }

    public static bool isSelectPressed()
    {
        if (Input.GetKey(altKey) && Input.GetKeyDown(KeyCode.S))
        {
            return true;
        }
        return false;
    }

    public static bool canMove()
    {
        return !Input.GetKey(ctrlKey) && !Input.GetKey(altKey);
    }

    public void Awake()
    {
        instance = this;
        LogInfo("Hello, the Instagram community!");
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

        BundlesManager.Load();
        EditorVariablesList.SetupEditorVariables();
        EditorComponentsList.SetupEditorComponents();

        var harmony = new Harmony("duviz.ultrakill.ultraeditor");
        harmony.PatchAll();

        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        SceneManager.sceneLoaded += (_, _) => new GameObject("load pls uwu :3").AddComponent<EmptySceneLoader>();
    }

    public void Start()
    {
        GameObject obj = new("ChapterSelectChanger", typeof(ChapterSelectChanger));
    }

    public void Update()
    {
        if (Input.GetKeyDown(editorOpenKey) && (SceneHelper.CurrentScene != EditorManager.EditorSceneName || EditorManager.canOpenEditor || EmptySceneLoader.forceLevelCanOpenEditor || (EditorManager.Instance != null && EditorManager.Instance.editorCanvas.activeInHierarchy)) && SceneHelper.PendingScene == null)
        {
            EditorManager.Create();
        }

        if (SceneHelper.CurrentScene == "Main Menu" && SceneHelper.PendingScene == null && !seenWelcomeMessage)
        {
            if (PlayerPrefs.GetString("UltraEditor_LastPlayedVersion") != GetVersion().ToString())
            {
                Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("WelcomeCanvas"));
                PlayerPrefs.SetString("UltraEditor_LastPlayedVersion", GetVersion().ToString());
            }
            seenWelcomeMessage = true;
        }
    }

    public static T Ass<T>(string path) { return Addressables.LoadAssetAsync<T>((object)path).WaitForCompletion(); }
    public static T Ast<T>(string path) where T : UnityEngine.Object
    {
        var obj = Resources.Load<T>(path);
        if (obj == null)
            LogError($"Resources.Load failed for '{path}'");
        return obj;
    }
    public static void LogInfo(object data, string stackTrace = null)
    {
        instance.Logger?.LogInfo(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (instance.Log ??= new("ULTRAEDITOR"))?.Info(data.ToString(), stackTrace: stackTrace);
    }
    public static void LogError(object data, string stackTrace = null)
    {
        instance.Logger?.LogError(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (instance.Log ??= new("ULTRAEDITOR"))?.Error(data.ToString(), stackTrace: stackTrace);
    }

    public static Version GetVersion()
    {
        return instance.Info.Metadata.Version;
    }
}