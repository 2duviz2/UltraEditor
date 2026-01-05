namespace UltraEditor.Classes.UI;

using System;
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
}