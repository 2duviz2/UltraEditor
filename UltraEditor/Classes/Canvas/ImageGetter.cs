namespace UltraEditor.Classes.Canvas;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
                tex.filterMode = FilterMode.Point;
                image.texture = tex;
            }
        }));
    }

    public static bool _loaded = true;
    public static List<(string, Texture2D)> cachedTextures = [];
    public static IEnumerator GetTextureFromURL(string url, System.Action<Texture2D> callback)
    {
        //while (!_loaded) yield return null;

        var TextureObjects = FindObjectsOfType<TextureObject>(true);
        foreach (TextureObject obj in TextureObjects)
        {
            if (obj != null)
            {
                if (obj.TextureName == url)
                {
                    if (obj.colonThree != null)
                    {
                        callback?.Invoke(obj.colonThree);
                        yield break;
                    }
                }
            }
        }

        (string, Texture2D) cached = cachedTextures.FirstOrDefault(x => x.Item1 == url);

        if (cached.Item2 != null)
        {
            callback(cached.Item2);
            yield break;
        }

        _loaded = false;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.timeout = 5;
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError($"Failed to load texture {url}: " + uwr.error);
                callback?.Invoke(null);
                _loaded = true;
            }
            else
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                cachedTextures.Add((url, tex));
                callback?.Invoke(tex);
                _loaded = true;
            }
        }
    }
}
