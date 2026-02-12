namespace UltraEditor.Classes;

using System.IO;
using System.Reflection;
using UnityEngine;

public static class BundlesManager
{
    public static AssetBundle editorBundle;

    public static GameObject editorCanvas, levelCanvas, exploreLevelsCanvas, welcomeCanvas;

    public static Shader ghostDottedOutline;

    public static GameObject cloudPrefab, pyramidMesh, duvizPlushPrefab, duvizPlushFixedPrefab;

    public static void Load()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UltraEditor.Assets.editorcanvas.bundle");

        AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(bundleStream);
        request.completed += (op) => 
        {
            editorBundle = request.assetBundle;
            if (editorBundle == null)
            {
                Plugin.LogError("Failed to load AssetBundle from memory!");
                return;
            }

            LoadAssets();
            Plugin.LogInfo("Loaded embedded AssetBundle!");
        };
    }

    public static void LoadAssets()
    {
        editorCanvas = editorBundle.LoadAsset<GameObject>("Assets/Prefabs/EditorCanvas.prefab");
        exploreLevelsCanvas = editorBundle.LoadAsset<GameObject>("ExploreLevelsCanvas");
        welcomeCanvas = editorBundle.LoadAsset<GameObject>("WelcomeCanvas");
        levelCanvas = editorBundle.LoadAsset<GameObject>("OpenLevelCanvas");

        ghostDottedOutline = editorBundle.LoadAsset<Shader>("GhostDottedOutline");

        cloudPrefab = editorBundle.LoadAsset<GameObject>("Cloud");
        pyramidMesh = editorBundle.LoadAsset<GameObject>("PyramidMesh");
        duvizPlushPrefab = editorBundle.LoadAsset<GameObject>("DuvizPlush");
        duvizPlushFixedPrefab = editorBundle.LoadAsset<GameObject>("DuvizPlushFixed");
    }
}