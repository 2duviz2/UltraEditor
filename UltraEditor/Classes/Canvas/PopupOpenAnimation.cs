namespace UltraEditor.Classes.Canvas;

using System.Collections.Generic;
using TMPro;
using UltraEditor.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class PopupOpenAnimation : MonoBehaviour
{
    RectTransform rectTransform;
    public List<Image> images;
    public List<TMP_Text> texts;

    float target = 1;
    public bool spawnedPopup = false;

    public void Start()
    {
        target = 1;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one * 0.5f;
        UpdateAlpha();
    }

    public void OnEnable()
    {
        Start();
    }

    public void Update()
    {
        rectTransform.localScale = rectTransform.localScale - (rectTransform.localScale - Vector3.one * target) / (target < 1 ? 5f : 10f / (TimePatcher.UnscaledDeltaTime() * 60));
        if (target < 1 && rectTransform.localScale.y < 0.52f)
        {
            if (!spawnedPopup)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }

        UpdateAlpha();
    }

    public void UpdateAlpha()
    {
        float a = (rectTransform.localScale.y - 0.5f) * 2;
        foreach (var img in images)
            img.color = img.color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, a);
        foreach (var tex in texts)
            tex.color = tex.color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, a);
    }

    public void Close()
    {
        target = 0.5f;
    }
}
