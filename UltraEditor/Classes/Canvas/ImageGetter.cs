using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    public class ImageGetter : MonoBehaviour
    {
        public string imageUrl;
        public RawImage image;

        public void SetImg()
        {
            StartCoroutine(GetTextureFromURL(imageUrl, tex =>
            {
                if (tex != null)
                {
                    image.texture = tex;
                }
            }));
        }

        public static bool _loaded = true;
        public static IEnumerator GetTextureFromURL(string url, System.Action<Texture2D> callback)
        {
            while (!_loaded) yield return null;
            _loaded = false;
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to load texture: " + uwr.error);
                    callback?.Invoke(null);
                    _loaded = true;
                }
                else
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                    callback?.Invoke(tex);
                    _loaded = true;
                }
            }
        }
    }
}
