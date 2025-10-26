using BepInEx;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace UltraEditor.Classes
{
    public class BundlesManager : MonoBehaviour
    {
        public static AssetBundle editorBundle;

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = "UltraEditor.Assets.editorcanvas.bundle";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Plugin.LogError($"Embedded resource '{resourceName}' not found!");
                    return;
                }

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                editorBundle = AssetBundle.LoadFromMemory(data);
                if (editorBundle != null)
                    Plugin.LogInfo("Loaded embedded AssetBundle!");
                else
                    Plugin.LogError("Failed to load AssetBundle from memory!");
            }
        }

        public void OnDestroy()
        {
            editorBundle?.Unload(false);
        }

    }
}