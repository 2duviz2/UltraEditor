namespace UltraEditor.Classes.Editor;

using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class ParseHelper
{
    public static Vector3 ParseVector3(string input)
    {
        input = input.Trim('(', ')');
        string[] parts = [.. input.Split(',', ' ').Where(p => !string.IsNullOrEmpty(p))];

        if (parts.Length != 3)
            throw new FormatException($"Invalid Vector3 format: {input}");

        return new(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture)
        );
    }

    public static Vector2 ParseVector2(string input)
    {
        input = input.Trim('(', ')');
        string[] parts = [.. input.Split(',', ' ').Where(p => !string.IsNullOrEmpty(p))];

        if (parts.Length != 2)
            throw new FormatException($"Invalid Vector2 format: {input}");

        return new(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture)
        );
    }


    public static Vector4 ParseVector4(string input)
    {
        input = input.Trim('(', ')', ' ');
        var parts = input.Split(',');

        if (parts.Length != 4)
            throw new FormatException($"Invalid Vector4 format: {input}");

        return new Vector4(
            float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)
        );
    }
}
