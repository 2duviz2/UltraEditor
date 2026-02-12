namespace UltraEditor.Classes;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetItem : MonoBehaviour
{
    public string assetPath;
    public string assetName;

    public TMP_Text assetNameText;
    public GameObject assetItemObject;

    public void Start()
    {
        if (string.IsNullOrEmpty(assetPath))
            assetPath = assetItemObject.name;
        else
            assetNameText.text = assetName;
        
        GetComponent<Button>()?.onClick.AddListener(() =>
        {
            EditorManager.Instance.SpawnAsset(assetPath);
        });
    }
}
