namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Changes the gravity when touched.")]
public class GravityTrigger : SavableObject
{
    [EditorVar("Gravity")]
    public Vector3 gravity = new Vector3(0, -40, 0);

    [EditorVar("Disable on trigger")]
    public bool disableOnTrigger = true;

    public static GravityTrigger Create(GameObject target) =>
        target.AddComponent<GravityTrigger>();

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
            Physics.gravity = gravity;
            if (disableOnTrigger)
                gameObject.SetActive(false);
        }
    }
}