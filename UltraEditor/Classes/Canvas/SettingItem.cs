namespace UltraEditor.Classes.Canvas;

using TMPro;
using UltraEditor.Libraries;
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
        bool currentValue = Preferences.GetInt(prefKey, defaultValue ? 1 : 0) == 1;
        Preferences.SetInt(prefKey, currentValue ? 0 : 1); // duviz what the fuck were you doing why is this an int
        UpdateValueText(); // WAIT WHAT THE FUCK WHY DOESNT PLAYERPREFS HAVE BOOLEANS??
    }

    public void UpdateValueText()
    {
        bool currentValue = Preferences.GetInt(prefKey, defaultValue ? 1 : 0) == 1;
        valueText.text = currentValue ? "True" : "False";
    }
}
