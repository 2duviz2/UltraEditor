namespace UltraEditor.Classes.UI;

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Library for building UI elements. </summary>
public static class Builder
{
    /// <summary> Creates a new gameobject with a recttransform and a component, then returns it for any further blah blah. </summary>
    public static T Manufacture<T>(string name, Transform parent, Rect rect, Action<T> act) where T : Component
    {
        RectTransform rectTransform = new GameObject(name).AddComponent<RectTransform>();
        rectTransform.parent = parent;
        rectTransform.localPosition = new(rect.x, rect.y);
        rectTransform.sizeDelta = new(rect.width, rect.height);

        T ComponentT = rectTransform.gameObject.AddComponent<T>();
        act(ComponentT);

        return ComponentT;
    }

    /// <summary> Creates some Text and stuff idk what im meant to tell you :P </summary>
    public static TextMeshProUGUI Text(string name, string text, Transform parent, Rect rect, float size = 27f, TextAlignmentOptions alignment = TextAlignmentOptions.Left, Color? col = null) =>
        Manufacture<TextMeshProUGUI>(name, parent, rect, textGui =>
        {
            textGui.alignment = alignment;
            textGui.color = col ?? Color.white;
            textGui.enableWordWrapping = true;
            textGui.font = Sprites.VCROSDMONO;
            textGui.fontSize = size;
            textGui.text = text;
        });

    /// <summary> Creates an Image with the given sprite and color. </summary>
    public static Image Image(string name, Transform parent, Rect rect, Sprite sprite = null, Color? col = null) =>
        Manufacture<Image>(name, parent, rect, image =>
        {
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.sprite = sprite ?? Sprites.Fill;
            image.color = col ?? Color.white;

            if (Sprites.SpriteVals.TryGetValue(sprite ?? Sprites.Fill, out var val)) 
                image.pixelsPerUnitMultiplier = val.ppu;
            else 
                image.pixelsPerUnitMultiplier = 4.05f;
        });

    /// <summary> Creates a Mask with the given sprite and color. </summary>
    public static Mask Mask(string name, Transform parent, Rect rect, Sprite sprite = null)
    {
        Image img = Image(name, parent, rect, sprite);
        Mask mask = img.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        return mask;
    }

    /// <summary> Creates a Button with the given sprite, color, and onClick Action. </summary>
    public static Button Button(string name, Transform parent, Rect rect, Sprite sprite = null, Color? col = null, Action onClick = null)
    {
        Image img = Image(name, parent, rect, sprite, col);
        Button button = img.gameObject.AddComponent<Button>();

        if (Sprites.SpriteVals.TryGetValue(sprite ?? Sprites.Fill, out var val))
            button.colors = val.cb;
        else
            button.colors = Sprites.FillColor;

        button.targetGraphic = img;
        button.onClick.AddListener(() => onClick());

        return button;
    }

    /// <summary> Creates an InputField with the given properties. </summary>
    public static TMP_InputField InputField(string name, string placeholder, Transform parent, Rect rect, Sprite sprite = null, Color? col = null, float size = 27f, TextAlignmentOptions alignment = TextAlignmentOptions.Left, Action<string> onEndEdit = null)
    {
        Image img = Image(name, parent, rect, sprite, col);
        TMP_InputField field = img.gameObject.AddComponent<TMP_InputField>();

        Mask mask = Mask("Viewport", img.transform, rect, sprite);
        TextMeshProUGUI placeholderComp = Text("Placeholder", placeholder, mask.transform, new(rect.x, rect.y, rect.width-5f, rect.height-5f), size, alignment);
        TextMeshProUGUI textComp = Text("Text", "", mask.transform, new(rect.x, rect.y, rect.width-5f, rect.height-5f), size, alignment);

        field.textComponent = textComp;
        field.placeholder = placeholderComp;
        if (onEndEdit != null) 
            field.onEndEdit.AddListener(txt => onEndEdit(txt));

        if (Sprites.SpriteVals.TryGetValue(sprite ?? Sprites.Fill, out var val))
            field.colors = val.cb;
        else
            field.colors = Sprites.FillColor;

        return field;
    }
}