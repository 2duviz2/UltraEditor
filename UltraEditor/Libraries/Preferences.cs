namespace UltraEditor.Libraries;

using UnityEngine;

public static class Preferences
{
    private static PrefsManager Prefs =>
        PrefsManager.Instance;

    public static void SetInt(string key, int value) =>
        Prefs.SetInt(key, value);

    public static int GetInt(string key, int fallback = 0)
    {
        // ultraeditor once used PlayerPrefs so auto-migrate those prefs :3c
        if (PlayerPrefs.HasKey(key))
        {
            int value = PlayerPrefs.GetInt(key);
            Prefs.SetInt(key, value);

            PlayerPrefs.DeleteKey(key);
            return value;
        }

        return Prefs.GetInt(key, fallback);
    }

    public static void SetString(string key, string value) =>
        Prefs.SetString(key, value);

    public static string GetString(string key, string fallback = null)
    {
        // ultraeditor once used PlayerPrefs so auto-migrate those prefs :3c
        if (PlayerPrefs.HasKey(key))
        {
            string value = PlayerPrefs.GetString(key);
            Prefs.SetString(key, value);

            PlayerPrefs.DeleteKey(key);
            return value;
        }

        return Prefs.GetString(key, fallback);
    }
}