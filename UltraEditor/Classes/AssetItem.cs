using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltraEditor.Classes
{
    public class AssetItem : MonoBehaviour
    {
        public string assetPath;
        public string assetName;

        public TMP_Text assetNameText;
        public GameObject assetItemObject;

        public void Start()
        {
            if (assetPath == "")
                assetPath = assetItemObject.name;
            else
                assetNameText.text = assetName;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                EditorManager.Instance.SpawnAsset(assetPath);
            });
        }
    }
}
