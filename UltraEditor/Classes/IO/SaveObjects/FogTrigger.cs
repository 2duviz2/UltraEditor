namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Changes the fog's color and distance when touched.")]
public class FogTrigger : SavableObject
{
    [EditorVar("Fog enabled")]
    public bool fogEnabled = true;

    [EditorVar("Color")]
    public Vector3 color = Vector3.oneVector * 255f;

    [EditorVar("Min distance")]
    public float minDistance = 0;

    [EditorVar("Max distance")]
    public float maxDistance = 100;

    [EditorVar("Disable on trigger")]
    public bool disableOnTrigger = true;

    public static FogTrigger Create(GameObject target) =>
        target.AddComponent<FogTrigger>();

    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        gameObject.GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            RenderSettings.fog = fogEnabled;
            RenderSettings.fogColor = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = minDistance;
            RenderSettings.fogEndDistance = maxDistance;
            if (disableOnTrigger)
                gameObject.SetActive(false);
        }
    }
}