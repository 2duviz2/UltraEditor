namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

public class HUDMessageObject : SavableObject
{
    public string message = "Default message";
    public bool disableAfterShowing = false;

    public static HUDMessageObject Create(GameObject target)
    {
        HUDMessageObject hudObject = target.AddComponent<HUDMessageObject>();
        return hudObject;
    }

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;
    }

    bool used = false;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (used) return;
            HudMessageReceiver.Instance.SendHudMessage(message);
            if (disableAfterShowing) used = true;
        }
    }
}