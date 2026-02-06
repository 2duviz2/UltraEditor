namespace UltraEditor.Classes.World;

using System;
using System.Reflection;
using UltraEditor.Classes.Canvas;
using UnityEngine;

public static class SkyboxManager
{
    const string prefix = "Assets/Materials/Skyboxes/";
    const string sufix = ".mat";

    public static string GetPathFromName(string name) =>
        $"Assets/Materials/Skyboxes/{name}.mat";

    public enum Skybox
    {
        BlackSky,
        [Path($"{prefix}DawnSky{sufix}")]
        DawnSky,
        [Path($"{prefix}DawnSky 1{sufix}")]
        DawnSky1,
        [Path($"{prefix}DaySky{sufix}")]
        DaySky,
        [Path($"{prefix}DaySky 1{sufix}")]
        DaySky1,
        [Path($"{prefix}DaySky 2{sufix}")]
        DaySky2,
        [Path($"{prefix}DuskSky{sufix}")]
        DuskSky,
        [Path($"{prefix}DuskSky Fog Variant{sufix}")]
        DuskSkyFog,
        [Path($"{prefix}DuskSky2{sufix}")]
        DuskSky2,
        [Path($"{prefix}DuskSky3{sufix}")]
        DuskSky3,
        [Path($"{prefix}EndlessSky{sufix}")]
        EndlessSky,
        [Path($"{prefix}EveningSky{sufix}")]
        EveningSky,
        [Path($"{prefix}FogCoveredSky{sufix}")]
        FogCoveredSky,
        [Path($"{prefix}GreedSky4{sufix}")]
        GreedSky4,
        [Path($"{prefix}NightSky1{sufix}")]
        NightSky1,
        [Path($"{prefix}NightSky2{sufix}")]
        NightSky2,
        [Path($"{prefix}NightSky3{sufix}")]
        NightSky3,
        [Path($"{prefix}RedSky{sufix}")]
        RedSky,
        [Path($"{prefix}RedSky2{sufix}")]
        RedSky2,
        [Path($"{prefix}SkyBlue{sufix}")]
        SkyBlue,
        [Path($"{prefix}ViolenceSky{sufix}")]
        ViolenceSky,
        [Path($"{prefix}ViolenceSky 1{sufix}")]
        ViolenceSky1,
        [Path($"{prefix}ViolenceSky4{sufix}")]
        ViolenceSky4,
        Custom,
    }

    public static void SetSkybox(Skybox skybox, string customUrl)
    {
        string path = GetPath(skybox);
        if (path == null)
        {
            switch (skybox)
            {
                case Skybox.Custom:
                    Plugin.instance.StartCoroutine(ImageGetter.GetTextureFromURL(customUrl, tex =>
                    {
                        if (tex != null)
                        {
                            var skyboxMat = new Material(Plugin.Ass<Shader>("Assets/Shaders/Special/Skybox_Panoramic.shader"));
                            skyboxMat.mainTexture = tex;
                            RenderSettings.skybox = skyboxMat;
                        }
                        else
                            RenderSettings.skybox = null;
                    }));
                    return;
                case Skybox.BlackSky:
                    RenderSettings.skybox = null;
                    return;
                default:
                    RenderSettings.skybox = null;
                    return;
            }
        }
        Material skyboxMaterial = Plugin.Ass<Material>(path);
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
        else
        {
            Debug.LogWarning($"Skybox material not found: {path}");
        }
    }


    public static string GetPath(Skybox skybox)
    {
        var field = typeof(Skybox).GetField(skybox.ToString());
        var attr = field.GetCustomAttribute<PathAttribute>();
        return attr?.Value;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class PathAttribute : Attribute
{
    public string Value { get; }

    public PathAttribute(string value)
    {
        Value = value;
    }
}