namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Changes the fog's color and distance when touched. Speed will make the fade faster (a speed of 10 takes longer than 20)")]
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

    [EditorVar("Fade speed")]
    public float fadeSpeed = 50;

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
            UpdateFog();
    }

    public void UpdateFog()
    {
        if (fogEnabled)
        {
            RenderSettings.fogColor = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
            RenderSettings.fogMode = FogMode.Linear;
        }

        if (RenderSettings.fog != fogEnabled)
        {
            if (fogEnabled)
                FogFadeController.instance.FadeIn(minDistance, maxDistance, fadeSpeed: fadeSpeed);
            else
                FogFadeController.instance.FadeOut(fadeSpeed: fadeSpeed);
        }

        if (disableOnTrigger)
            gameObject.SetActive(false);
    }
}