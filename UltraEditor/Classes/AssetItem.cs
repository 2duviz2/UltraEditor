namespace UltraEditor.Classes;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UltraEditor.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class AssetItem : MonoBehaviour
{
    public string assetPath;
    public string assetName;

    public TMP_Text assetNameText;
    public GameObject assetItemObject;
    public Image assetItemPreview;

    public void Start()
    {
        assetNameText.text = assetName;
        
        GetComponent<Button>()?.onClick.AddListener(() => 
        {
            // open the folder that this asset item is in if youre searching for it
            if (AssetsWindowManager.Instance.CurrentFolder.StartsWith("Search"))
            {
                AssetsWindowManager.Instance.CurrentFolder = Path.GetDirectoryName(assetPath).Replace('\\', '/') + '/';
                AssetsWindowManager.Instance.Refresh();
            }

            EditorManager.Instance.SpawnAsset(assetPath);
        });
    }

    public void SetPreview(string path) =>
        StartCoroutine(WaitForPreview(path));

    public static Dictionary<string, Sprite> cachedPreviews = [];
    IEnumerator WaitForPreview(string path)
    {
        if (assetItemPreview == null) 
            yield break;

#if EXPORTMODE
        yield break;
#endif

        if (cachedPreviews.TryGetValue(path, out Sprite cachedAsset))
        {
            assetItemPreview.sprite = cachedAsset;
            yield break;
        }

        var spr = BundlesManager.editorBundle.LoadAsset<Sprite>(path.Replace("/", "-"));

        if (spr != null)
        {
            assetItemPreview.sprite = spr;
            cachedPreviews[path] = assetItemPreview.sprite;
            yield break;
        }

        GameObject previewObj = EditorManager.Instance.SpawnAsset(path, isLoading: true);

        SetLayerRecursively(previewObj, LayerMask.NameToLayer("Invisible"));

        Bounds b = GetBoundsRecursive(previewObj);
        Vector3 center = b.center;
        float size = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);

        float distance = 5;
        if (size > 5)
            distance = 15;

        previewObj.transform.position = new Vector3(0, -10000, distance);
        if (size > 5)
            previewObj.transform.Rotate(0, 180, 0);

        AssetsWindowManager.Instance.PreviewCamera.enabled = false;
        AssetsWindowManager.Instance.PreviewCamera.Render();

        assetItemPreview.sprite = RenderTextureToSprite(AssetsWindowManager.Instance.PreviewTexture);

        cachedPreviews[path] = assetItemPreview.sprite;

        DestroyImmediate(previewObj);
    }

    public static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public Bounds GetBoundsRecursive(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    public static Sprite RenderTextureToSprite(RenderTexture rt)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        RenderTexture.active = prev;

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}