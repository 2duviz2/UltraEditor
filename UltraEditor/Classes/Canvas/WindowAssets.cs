namespace UltraEditor.Classes.Canvas;

using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowAssets : MonoBehaviour
{
    [Serializable]
    public class asset
    {
        public string name;
        public GameObject obj;
    }

    public List<asset> assets = [];
}
