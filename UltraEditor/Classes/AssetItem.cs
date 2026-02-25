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
    public Image assetItemPreview;

    public void Start()
    {
        assetNameText.text = assetName;
        
        GetComponent<Button>()?.onClick.AddListener(() => EditorManager.Instance.SpawnAsset(assetPath));
    }
}
