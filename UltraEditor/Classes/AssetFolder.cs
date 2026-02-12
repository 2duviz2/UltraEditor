namespace UltraEditor.Classes;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetFolder : MonoBehaviour
{
    public string folderPath;

    public TMP_Text folderNameText;

    public void Start()
    {
        if (string.IsNullOrEmpty(folderPath))
            folderPath = folderNameText.name;
        else
            folderNameText.text = folderPath;
        
        GetComponent<Button>()?.onClick.AddListener(() =>
        {
            //EditorManager.Instance.SpawnAsset(assetPath);
        });
    }
}
