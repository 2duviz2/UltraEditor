namespace UltraEditor.Classes.UI;

using System.Collections.Generic;
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

    /// <summary> ColorBlock's Used For Buttons And The Such. </summary>
    public static ColorBlock FillColor, BorderColor;

    /// <summary> Gets the colorblock for a specific sprite as different sprites use different colors. </summary>
    public static Dictionary<Sprite, ColorBlock> SpriteColors = [];

    /// <summary> Loads all the sprites and color blocks. </summary>
    public static void Load()
    {
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

        // Load which colorblocks correspond to which sprites
        AddColor(BorderColor, Border, BorderBlack, SmallBorder);
        AddColor(FillColor, Fill, SmallFill);
    }

    /// <summary> Adds a ColorBlock for a sprite since different sprites have different colors. </summary>
    public static void AddColor(ColorBlock cb, params List<Sprite> sprites) =>
        sprites.ForEach(s => SpriteColors.Add(s, cb));

    /// <summary> Just loads a sprite from UI addressables synchronized. </summary>
    public static Sprite LoadSprite(string key) =>
        Addressables.LoadAssetAsync<Sprite>($"Assets/Textures/UI/Controls/{key}.png").WaitForCompletion();
}