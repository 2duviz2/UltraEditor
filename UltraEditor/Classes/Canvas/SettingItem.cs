namespace UltraEditor.Classes.Canvas;

using TMPro;
using UnityEngine;

public class SettingItem : MonoBehaviour
{
    public string prefKey = "TestSetting";
    public bool defaultValue = false;
    public TMP_Text valueText;

    public void Awake()
    {
        UpdateValueText();
    }

    public void ToggleValue()
    {
        bool currentValue = PlayerPrefs.GetInt(prefKey, defaultValue ? 1 : 0) == 1;
        PlayerPrefs.SetInt(prefKey, currentValue ? 0 : 1);
        UpdateValueText();
    }

    public void UpdateValueText()
    {
        bool currentValue = PlayerPrefs.GetInt(prefKey, defaultValue ? 1 : 0) == 1;
        valueText.text = currentValue ? "True" : "False";
    }
}
