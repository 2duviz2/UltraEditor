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

    public static List<(string, Texture2D)> cachedTextures = [];
    public static IEnumerator GetTextureFromURL(string url, System.Action<Texture2D> callback)
    {
        if (TextureObject.textures.ContainsKey(url))
        {
            callback(TextureObject.textures[url]);
            yield break;
        }

        (string, Texture2D) cached = cachedTextures.FirstOrDefault(x => x.Item1 == url);

        if (cached.Item2 != null)
        {
            callback(cached.Item2);
            yield break;
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.timeout = 5;
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError($"Failed to load texture {url}: " + uwr.error);
                callback?.Invoke(null);
            }
            else
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                cachedTextures.Add((url, tex));
                callback?.Invoke(tex);
            }
        }
    }
}
