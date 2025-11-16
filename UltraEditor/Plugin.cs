using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UltraEditor.Classes;
using UnityEngine.SceneManagement;

namespace UltraEditor
{
    [BepInPlugin("duviz.ultrakill.ultraeditor", "UltraEditor", "0.0.4")]
    public class Plugin : BaseUnityPlugin
    {
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

            /*((UnityEngine.Object)((Component)this).gameObject).hideFlags = (HideFlags)61;
            ((UnityEngine.Object)ThreadingHelper.Instance).hideFlags = (HideFlags)61;
            ((ConfigEntry<bool>)AccessTools.Field(typeof(Chainloader), "ConfigHideBepInExGOs").GetValue(null)).Value = true;*/

            LogInfo("Hello, the Instagram community!");

            var harmony = new Harmony("duviz.ultrakill.ultraeditor");
            harmony.PatchAll();

            gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }

        public void Start()
        {
            GameObject obj = new GameObject("BundlesManager");
            obj.AddComponent<BundlesManager>();
        }

        public void Update()
        {
            if (Input.GetKeyDown(editorOpenKey))
            {
                EditorManager.Create();
            }
        }

        public void LateUpdate()
        {

        }

        public static T Ass<T>(string path) { return Addressables.LoadAssetAsync<T>((object)path).WaitForCompletion(); }
        public static T Ast<T>(string path) where T : UnityEngine.Object
        {
            var obj = Resources.Load<T>(path);
            if (obj == null)
                LogError($"Resources.Load failed for '{path}'");
            return obj;
        }
        public static void LogInfo(object data) 
        {
            instance.Logger?.LogInfo(data);
            (instance.Log ??= new("ULTRAEDITOR"))?.Info(data.ToString());
        }
        public static void LogError(object data) 
        { 
            instance.Logger?.LogError(data);
            (instance.Log ??= new("ULTRAEDITOR"))?.Error(data.ToString());
        }
    }
}