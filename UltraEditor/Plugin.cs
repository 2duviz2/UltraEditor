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
    public const string Version = "0.1.0";

    public static Plugin Instance;
    public plog.Logger Log;

    public static KeyCode EditorOpenKey = KeyCode.F1;
    public static KeyCode SelectCursorKey = KeyCode.F2;
    public static KeyCode SelectMoveKey = KeyCode.F3;
    public static KeyCode SelectScaleKey = KeyCode.F4;
    public static KeyCode SelectRotationKey = KeyCode.F5;
    public static KeyCode ToggleEditorCanvasKey = KeyCode.F9;
    public static KeyCode DeleteObjectKey = KeyCode.Delete;
    public static KeyCode CreateCubeKey = KeyCode.KeypadPlus;
    public static KeyCode CtrlKey = KeyCode.LeftControl;
    public static KeyCode ShiftKey = KeyCode.LeftShift;
    public static KeyCode AltKey = KeyCode.LeftAlt;

    const string LastPlayedVersionPlayerPrefs = "UltraEditor_LastPlayedVersion";

    static bool SeenWelcomeMessage = false;

    public static bool IsToggleEnabledKeyPressed() =>
        Input.GetKey(AltKey) && Input.GetKey(ShiftKey) && Input.GetKeyDown(KeyCode.A);

    public static bool IsDuplicateKeyPressed() =>
        Input.GetKey(CtrlKey) && Input.GetKeyDown(KeyCode.D);

    public static bool IsUndoPressed() =>
        Input.GetKey(CtrlKey) && Input.GetKeyDown(KeyCode.Z);

    public static bool IsRedoPressed() =>
        Input.GetKey(CtrlKey) && Input.GetKeyDown(KeyCode.R);

    public static bool IsSelectPressed()
    {
        if (Input.GetKey(AltKey) && Input.GetKeyDown(KeyCode.S))
        {
            return true;
        }
        return false;
    }

    public static bool CanMove() =>
        !Input.GetKey(CtrlKey) && !Input.GetKey(AltKey);

    public void Awake()
    {
        Instance = this;
        LogInfo("Hi :3");
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

        // load the assets window
        AssetsWindowManager.Load();
    }

    public void Update()
    {
        if (Input.GetKeyDown(EditorOpenKey) && (SceneHelper.CurrentScene != EditorManager.EditorSceneName || EditorManager.canOpenEditor || EmptySceneLoader.forceLevelCanOpenEditor || (EditorManager.Instance != null && EditorManager.Instance.editorCanvas.activeInHierarchy)) && SceneHelper.PendingScene == null)
        {
            EditorManager.Create();
        }

        if (SceneHelper.CurrentScene == "Main Menu" && SceneHelper.PendingScene == null && !SeenWelcomeMessage)
        {
            if (PlayerPrefs.GetString(LastPlayedVersionPlayerPrefs) != GetVersion().ToString())
            {
                Instantiate(BundlesManager.welcomeCanvas);
                PlayerPrefs.SetString(LastPlayedVersionPlayerPrefs, GetVersion().ToString());
            }
            SeenWelcomeMessage = true;
        }
    }

    [Obsolete("Use AddressablesHelper")]
    public static T Ass<T>(string path) { return Addressables.LoadAssetAsync<T>((object)path).WaitForCompletion(); }
    [Obsolete("Use AddressablesHelper")]
    public static T Ast<T>(string path) where T : UnityEngine.Object
    {
        T obj = Resources.Load<T>(path);
        if (obj == null)
            LogError($"Resources.Load failed for \"{path}\"");
        return obj;
    }

    #region Logging
    public static void LogInfo(object data, string stackTrace = null)
    {
        Instance.Logger?.LogInfo(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (Instance.Log ??= new("ULTRAEDITOR"))?.Info(data.ToString(), stackTrace: stackTrace);
    }
    public static void LogError(object data, string stackTrace = null)
    {
        Instance.Logger?.LogError(stackTrace != null
            ? $"{data}\n{stackTrace}"
            : data);

        (Instance.Log ??= new("ULTRAEDITOR"))?.Error(data.ToString(), stackTrace: stackTrace);
    }
    #endregion

    public static Version GetVersion()
    {
        return Instance.Info.Metadata.Version;
    }
}