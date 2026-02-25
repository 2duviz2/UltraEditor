namespace UltraEditor.Libraries;

using System.IO;
using UnityEngine;

#if EXPORTMODE
public static class SpriteExporter
{
    public static void ExportTexture(Sprite sprite, string path)
    {
        if (sprite == null)
            return;

        Texture2D tex = sprite.texture;

        Rect rect = sprite.rect;
        Texture2D cropped = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);

        Color[] pixels = tex.GetPixels(
            (int)rect.x,
            (int)rect.y,
            (int)rect.width,
            (int)rect.height
        );
        cropped.SetPixels(pixels);
        cropped.Apply();

        byte[] pngData = cropped.EncodeToPNG();
        string folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "CachedImages");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, path.Replace("/", "-") + ".png");
        File.WriteAllBytes(filePath, pngData);
    }
}
#endif