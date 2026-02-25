namespace UltraEditor.Libraries;

using System.Collections.Generic;

public static class PrefabFixer
{
    public static Dictionary<string, string> replaceKeys = new()
    {
        ["Assets/Prefabs/Enemies/Projectile Zombie.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Stray.prefab",
        ["Assets/Prefabs/Enemies/ShotgunHusk.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Soldier.prefab",
        ["Assets/Prefabs/Enemies/Super Projectile Zombie.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Schism.prefab",
        ["Assets/Prefabs/Enemies/Zombie.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Filth.prefab",
        ["Assets/Prefabs/Enemies/Mass.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Statue/Mass.prefab",
        ["Assets/Prefabs/Enemies/Flesh Prison.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Statue/Flesh Prison.prefab",
        ["Assets/Prefabs/Enemies/Flesh Prison 2.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Statue/Flesh Panopticon.prefab",
        ["Assets/Prefabs/Enemies/Cerberus.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Statue/Cerberus.prefab",
        ["Assets/Prefabs/Enemies/SwordsMachineNonboss.prefab"] = "Assets/Prefabs/Enemies/Rewrite/Machine/Swordsmachine.prefab",
        ["Assets/Prefabs/Enemies/Spider.prefab"] = "Assets/Prefabs/Enemies/Malicious Face.prefab",
        ["Assets/Prefabs/Enemies/Gabriel 2nd Variant.prefab"] = "Assets/Prefabs/Enemies/Gabriel 2nd.prefab",
    };

    public static string FixKey(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        foreach (var pair in replaceKeys)
        {
            text = text.Replace(pair.Key, pair.Value);
        }

        return text;
    }
}