namespace UltraEditor.Classes.IO.SaveObjects;

using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using UnityEngine;

[EditorComp("Creates 2 portals.")]
public class PortalObject : SavableObject
{
    [EditorVar("Portal Width")]
    public float PortalWidth = 5;

    [EditorVar("Portal Height")]
    public float PortalHeight = 7.5f;

    [EditorVar("Max Recursions")]
    public int MaxRecursions = 3;

    /// <summary> The entrance portal. </summary>
    public GameObject PortalEntrance;

    /// <summary> The exit portal. </summary>
    public GameObject PortalExit;
    
    /// <summary> Portal component mrrrp miaow </summary>
    public Portal portal;

    public Vector3 PortalEntrancePos = Vector3.zero, PortalEntranceRot = Vector3.zero, PortalEntranceSca = Vector3.zero;
    public Vector3 PortalExitPos = Vector3.back * 10, PortalExitRot = Vector3.up * 180, PortalExitSca = Vector3.zero;

    /// <summary> Creates the portals and setups the portal meowmeowmeow </summary>
    public void Start()
    {
        DisableCollision();

        // create the entrace portal if it doesnt exist
        if (!PortalEntrance)
        {
            PortalEntrance = new("Portal Enterance", typeof(SavableObject));
            PortalEntrance.SetActive(false);

            PortalEntrance.transform.parent = transform;
            PortalEntrance.transform.localPosition = PortalEntrancePos;
            PortalEntrance.transform.localScale = PortalEntranceSca;
            PortalEntrance.transform.forward = new(0f, 0f, 1f);
            PortalEntrance.transform.eulerAngles = PortalEntranceRot;
        }

        // create exit portal if it doesnt exist
        if (!PortalExit)
        {
            PortalExit = new("Portal Exit", typeof(SavableObject));
            PortalExit.SetActive(false);

            PortalExit.transform.parent = transform;
            PortalExit.transform.localPosition = PortalExitPos;
            PortalExit.transform.localScale = PortalExitSca;
            PortalExit.transform.forward = new(0f, 0f, 1f);
            PortalExit.transform.eulerAngles = PortalExitRot;
        }

        // set up the portals
        portal = gameObject.AddComponent<Portal>();
        portal.shape = new PlaneShape() { width = PortalWidth, height = PortalHeight };

        // set transforms for the entry/exit portals
        portal.entry = PortalEntrance.transform;
        portal.exit = PortalExit.transform;

        // bleeehhhh extra stuff :P
        portal.maxRecursions = MaxRecursions;
        portal.supportInfiniteRecursion = true;
        portal.renderSettings = (PortalSideFlags)(-1);

        // re-enable the portals
        PortalEntrance.SetActive(true);
        PortalExit.SetActive(true);
    }

    /// <summary> rawr </summary>
    public override void Tick()
    {
        if (portal.shape is PlaneShape shape && (shape.width != PortalWidth || shape.height != PortalHeight))
            portal.shape = new PlaneShape() 
                { width = PortalWidth, height = PortalHeight };
    }
}