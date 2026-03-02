namespace UltraEditor.Classes.IO.SaveObjects;

using Unity.AI.Navigation;
using UnityEngine;

[EditorComp("Makes the player orbit around an object when touched.")]
public class OrbitTrigger : SavableObject
{
    [EditorVar("To Orbit")]
    public GameObject[] ToOrbit = [];

    [EditorVar("Gravity Strength")]
    public float GravityStrength = 40f;

    /// <summary> Whether we are currently orbiting the object. </summary>
    public bool orbitting = false;

    /// <summary> NewMovement instance property cuz i don't wanna type out NewMovement.Instance every time :P </summary>
    public static NewMovement nm => NewMovement.Instance;

    /// <summary> Sets up some extra stuff for the GameObject. </summary>
    public override void Create()
    {
        NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
        
        gameObject.GetComponent<Collider>()?.isTrigger = true;
    }

    /// <summary> Actually orbit the obj :3 </summary>
    public override void Tick()
    {
        if (orbitting && ToOrbit.Length == 1)
            nm.SwitchGravity((nm.transform.position - ToOrbit[0].transform.position).normalized * -GravityStrength);
    }

    /// <summary> Makes us start orbiting when we touch the trigger. </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            orbitting = true;
    }

    /// <summary> Makes us stop orbiting when we exit the trigger. </summary>
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            orbitting = false;
            nm.ResetGravity();
        }
    }
}