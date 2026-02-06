namespace UltraEditor.Classes.IO.SaveObjects;

using UnityEngine;

public class PrefabObject : SavableObject
{
    public string PrefabAsset;

    public static PrefabObject Create(GameObject target, string path)
    {
        PrefabObject obj = target.AddComponent<PrefabObject>();
        obj.PrefabAsset = path;
        return obj;
    }

    public override void Create()
    {

    }
}
