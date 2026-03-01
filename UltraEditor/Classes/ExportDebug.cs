namespace UltraEditor.Classes;

using System;
using System.Collections.Generic;
using System.Text;
using UltraEditor.Libraries;
using UnityEngine;

public class ExportDebug : MonoBehaviour
{
#if EXPORTMODE
    GameObject previewObj;
    string previewPath = "NONE";
    List<string> paths = [];

    public int r = 0;
    public int g = 0;
    public int b = 0;

    float offset = 0;

    public void Update()
    {
        AssetsWindowManager.Instance.PreviewCamera.backgroundColor = new Color(r, g, b, 1);
        AssetsWindowManager.Instance.PreviewCamera.Render();

        if (Input.GetKeyDown(KeyCode.T)) AssetsWindowManager.Instance.PreviewCamera.transform.rotation = Quaternion.identity;
        if (Input.GetKeyDown(KeyCode.T)) AssetsWindowManager.Instance.PreviewCamera.transform.position = new Vector3(0, -10000 - offset, 0);
        if (Input.GetKeyDown(KeyCode.R)) r = (r + 1) % 2;
        if (Input.GetKeyDown(KeyCode.G)) g = (g + 1) % 2;
        if (Input.GetKeyDown(KeyCode.B)) b = (b + 1) % 2;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            AssetsWindowManager.Folders.TryGetValue(AssetsWindowManager.Instance.CurrentFolder, out List<string> keys);
            if (keys != null) paths = keys;
        }

        if (previewObj == null || Input.GetKeyDown(KeyCode.X))
        {
            AssetsWindowManager.Instance.PreviewCamera.backgroundColor = new Color(0, 0, 0, 0);
            AssetsWindowManager.Instance.PreviewCamera.Render();
            if (previewObj != null)
                SpriteExporter.ExportTexture(RenderTextureToSprite(AssetsWindowManager.Instance.PreviewTexture), previewPath.Replace("/", "-"));
            NextPreview();
        }

        if (Input.GetKeyDown(KeyCode.C))
            NextPreview();

        if (previewObj == null) return;

        float speed = 10 * (Input.GetKey(KeyCode.LeftShift) ? 3 : 1f) * Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        float horizontal = Input.GetAxisRaw("Horizontal") * speed;
        float vertical = Input.GetAxisRaw("Vertical") * speed;
        float ascend = (Input.GetKey(KeyCode.E) ? 1 : 0 - (Input.GetKey(KeyCode.Q) ? 1 : 0)) * speed;
        AssetsWindowManager.Instance.PreviewCamera.transform.Translate(new Vector3(horizontal, ascend, vertical));

        float xRot1 = Input.GetKey(KeyCode.L) ? 45 : 0;
        float xRot2 = Input.GetKey(KeyCode.J) ? -45 : 0;

        float yRot1 = Input.GetKey(KeyCode.I) ? 45 : 0;
        float yRot2 = Input.GetKey(KeyCode.K) ? -45 : 0;

        float zRot1 = Input.GetKey(KeyCode.O) ? 45 : 0;
        float zRot2 = Input.GetKey(KeyCode.U) ? -45 : 0;

        AssetsWindowManager.Instance.PreviewCamera.transform.Rotate((xRot1 + xRot2) * Time.unscaledDeltaTime, (yRot1 + yRot2) * Time.unscaledDeltaTime, (zRot1 + zRot2) * Time.unscaledDeltaTime);
    }

    public void NextPreview()
    {
        offset -= 100;
        AssetsWindowManager.Instance.PreviewCamera.cullingMask = CameraController.Instance.cam.cullingMask;
        if (previewObj != null)
            Destroy(previewObj);
        if (paths.Count == 0) return;
        previewPath = paths[0];
        NewPreview(paths[0]);
        paths.RemoveAt(0);
    }

    public void NewPreview(string path)
    {
        if (path.Contains("Puppet")) return;
        previewObj = EditorManager.Instance.SpawnAsset(path, isLoading: true);

        Bounds b = GetBoundsRecursive(previewObj);
        float size = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);

        float distance = 5;
        if (size > 5)
            distance = 15;

        previewObj.transform.position = new Vector3(0, -10000 - offset, distance);
        if (size > 5)
            previewObj.transform.Rotate(0, 180, 0);
        AssetsWindowManager.Instance.PreviewCamera.transform.position = new Vector3(0, -10000 - offset, 0);
        AssetsWindowManager.Instance.PreviewCamera.transform.rotation = Quaternion.identity;

        foreach (Transform child in previewObj.transform)
        {
            if (child.name.Contains("SpawnEffect"))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public void OnGUI()
    {
        if (AssetsWindowManager.Instance.PreviewTexture == null) return;

        float maxHeight = Screen.height;
        float maxWidth = Screen.width;
        float aspect = (float)AssetsWindowManager.Instance.PreviewTexture.width / AssetsWindowManager.Instance.PreviewTexture.height;
        float drawHeight = Mathf.Min(maxHeight, maxWidth / aspect);
        float drawWidth = drawHeight * aspect;

        Rect bgRect = new(
            0,
            0,
            Screen.width,
            Screen.height
        );
        Color oldColor = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
        GUI.color = oldColor;

        Rect rect = new(
            (Screen.width - drawWidth) / 2f,
            (Screen.height - drawHeight) / 2f,
            drawWidth,
            drawHeight
        );

        Rect textRect = new(0, 0, 300, 300);

        GUI.DrawTexture(rect, AssetsWindowManager.Instance.PreviewTexture, ScaleMode.ScaleToFit, true);
        GUI.Label(textRect, $"Current: {previewPath}");
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
#endif
}
