namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections.Generic;
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

    /// <summary> Creates the portals and setups the portal meowmeowmeow </summary>
    public void Start()
    {
        // create the entrace portal if it doesnt exist
        if (!PortalEntrance)
        {
            PortalEntrance = new("Portal Enterance", typeof(SavableObject));
            PortalEntrance.SetActive(false);

            PortalEntrance.transform.parent = transform;
            PortalEntrance.transform.localPosition = new(0f, 0f, 10f);
            PortalEntrance.transform.localScale = new(5f, 5f, 5f);
            PortalEntrance.transform.forward = new(0f, 0f, 1f);
        }

        // create exit portal if it doesnt exist
        if (!PortalExit)
        {
            PortalExit = new("Portal Exit", typeof(SavableObject));
            PortalExit.SetActive(false);

            PortalExit.transform.parent = transform;
            PortalExit.transform.localPosition = new(0f, 0f, -10f);
            PortalExit.transform.localScale = new(5f, 5f, 5f);
            PortalExit.transform.forward = new(0f, 0f, 1f);
            PortalExit.transform.eulerAngles = new(0f, 180f, 0f);
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
            portal.shape = new PlaneShape() { width = PortalWidth, height = PortalHeight };
    }

    /// <summary> Line material for drawing the outline. </summary>
    public Material LineMat;

    /// <summary> Render the outline for the portal if we have the editor open. </summary>
    public void OnRenderObject()
    {
        if (!EditorManager.Instance.editorOpen)
            return;

        // portals width and height are divided by 5 :3
        float width =  (PortalWidth /5f) / 2f;
        float height = (PortalHeight/5f) / 2f;

        DrawOutline(PortalEntrance.transform);
        DrawOutline(PortalExit.transform);

        void DrawOutline(Transform local)
        {
            if (!(LineMat ??= new(DefaultReferenceManager.Instance.masterShader)).SetPass(0))
                return;

            GL.PushMatrix();
            GL.MultMatrix(local.localToWorldMatrix);

            DrawLines(new(0f, 1f, 0.75f),
            [
                new(width, height, 0),
                new(width, -height, 0),
                
                new(-width, -height, 0),
                new(width, -height, 0),
                
                new(-width, -height, 0),
                new(-width, height, 0),

                new(-width, height, 0),
                new(width, height, 0),
            ]);

            GL.PopMatrix();
        }
    }

    /// <summary> Draws a buncha lines like a gizmo :sleepingfilth: </summary>
    public void DrawLines(Color col, params List<Vector3> linePositions)
    {
        GL.Begin(GL.LINES);
        GL.Color(col);

        linePositions.ForEach(GL.Vertex);

        GL.End();
    }
}