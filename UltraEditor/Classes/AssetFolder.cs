namespace UltraEditor.Classes;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetFolder : MonoBehaviour
{
    public string folderPath;
    public string folderName;

    public TMP_Text folderNameText;

    public void Start()
    {
        if (!string.IsNullOrEmpty(folderPath))
            folderNameText.text = folderName;
        
        GetComponent<Button>()?.onClick.AddListener(() =>
        {
            //EditorManager.Instance.SpawnAsset(assetPath);
        });
    }
}
