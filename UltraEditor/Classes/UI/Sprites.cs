namespace UltraEditor.Classes.UI;

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary> Loads and remembers all the sprites and color blocks for the UI. </summary>
public static class Sprites
{
    /// <summary> Octagonal/Cut Corner UI Sprites. </summary>
    public static Sprite Border, BorderBlack, Fill;

    /// <summary> Small Octagonal/Cut Corner UI Sprites. </summary>
    public static Sprite SmallBorder, SmallFill;

    /// <summary> Ultrakill's font. </summary>
    public static TMP_FontAsset VCROSDMONO;

    /// <summary> ColorBlock's Used For Buttons And The Such. </summary>
    public static ColorBlock FillColor, BorderColor;

    /// <summary> Used for getting any sprite specific values such as ColorBlock's or PixelPerUnit. </summary>
    public static Dictionary<Sprite, (float ppu, ColorBlock cb)> SpriteVals = [];

    /// <summary> Loads all the UI things. </summary>
    public static void Load()
    {
        // Load Font
        VCROSDMONO = Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Fonts/VCR_OSD_MONO_UI.asset").WaitForCompletion();

        // Create ColorBlocks
        FillColor = BorderColor = ColorBlock.defaultColorBlock;
        BorderColor.highlightedColor = BorderColor.selectedColor = new(.5f, .5f, .5f, 1f);
        FillColor.pressedColor = BorderColor.pressedColor = new(1f, 0f, 0f, 1f);

        // Load all the sprites and assign their colors
        Border = LoadSprite("Round_BorderLarge");
        BorderBlack = LoadSprite("Round_BorderLargeBlack");
        Fill = LoadSprite("Round_FillLarge");

        SmallBorder = LoadSprite("Round_BorderSmall");
        SmallFill = LoadSprite("Round_FillSmall");

        // Load sprite specific values
        RegisterValues(4.05f, BorderColor, Border, BorderBlack);
        RegisterValues(4.05f, FillColor, Fill);
        RegisterValues(5.4f, BorderColor, SmallBorder);
        RegisterValues(5.4f, FillColor, SmallFill);
    }

    /// <summary> Registers different sprite specific values. </summary>
    public static void RegisterValues(float pixelsPerUnit, ColorBlock colorBlock, params List<Sprite> sprites) =>
        sprites.ForEach(s => SpriteVals.Add(s, (pixelsPerUnit, colorBlock)));

    /// <summary> Just loads a sprite from UI addressables synchronized. </summary>
    public static Sprite LoadSprite(string key) =>
        Addressables.LoadAssetAsync<Sprite>($"Assets/Textures/UI/Controls/{key}.png").WaitForCompletion();
}