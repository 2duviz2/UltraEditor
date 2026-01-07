using UltraEditor.Classes.Canvas;
using UnityEngine;

namespace UltraEditor.Classes.Editor
{
    public static class PopupManager
    {
        static string popupPrefab = "Popup";

        /// <summary> Creates a popup with a text and content </summary>
        /// <param name="title"> Title of the popup </param>
        /// <param name="content"> Content of the popup </param>
        public static void CreatePopup(string title, string content)
        {
            GameObject spawnedObject = GameObject.Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>(popupPrefab), EditorManager.Instance.editorCanvas.transform.GetChild(0));
            PopupObject po = spawnedObject.GetComponent<PopupObject>();
            po.SetPopupInfo(title, content);
        }
    }
}