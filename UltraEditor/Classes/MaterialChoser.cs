namespace UltraEditor.Classes;

using System;
using System.Collections.Generic;
using UltraEditor.Classes.Canvas;
using UltraEditor.Libraries;
using UnityEngine;

public class MaterialChoser : MonoBehaviour
{
    public Vector2 tile = Vector2.one;
    public Vector2 offset = Vector2.one;
    private Vector3 lastScale = Vector3.one;
    private Mesh mesh;

    public enum materialTypes
    {
        Default,
        Armor,
        Glass,
        Grass,
        Metal,
        Wood,
        MasterShader,
        NoCollision,
        Brick,
        TilesDirty,
        ConstructBuilding,
        BloodPool,
        CyberGrind,
        wip,
        Flesh,
        BrickLight,
        Sand,
        CaveRock,
        FerryFloor,
        BoneWall,
        WhiteBookshelf,
        WhiteFlowers,
        WarningStripes,
        WhiteWood,
        UnbreakableGlass,
        LightGlow,
        Bush,
        Dirt,
        LimboGlass1,
        LimboGlass2,
        LimboGlass3,
        LimboGlass4,
        StoneMossy,
        Mulch,
        Metal1,
        Metal2,
        Metal3,
        Metal4,
        Metal5,
        Metal6,
        Metal7,
        Metal8,
    }

    public enum shapes
    {
        Cube,
        Pyramid,
        /*Sphere,
        Capsule,
        Plane,*/
    }

    public static MaterialChoser Create(GameObject target, materialTypes materialType)
    {
        MaterialChoser obj = target.AddComponent<MaterialChoser>();
        obj.ProcessMaterial(materialType);
        return obj;
    }

    shapes lastShape = shapes.Cube;
    public void ProcessMaterial(materialTypes type, float tiling = 0.25f, shapes? shape = null, bool fixMaterialTiling = false, string customTexture = "")
    {
        var renderer = GetComponent<Renderer>();
        if (!renderer) return;

        var collider = GetComponent<Collider>();
        if (collider) collider.enabled = true;

        Material newMat = null;
        lastScale = Vector3.zero;

        if (shape != null)
            if ((shapes)shape != lastShape)
            {
                lastShape = (shapes)shape;
                GameObject tempObj = GameObject.CreatePrimitive(
                      (shapes)shape == shapes.Cube ? PrimitiveType.Cube
                    /*: (shapes)shape == shapes.Sphere ? PrimitiveType.Sphere
                    : (shapes)shape == shapes.Capsule ? PrimitiveType.Capsule
                    : (shapes)shape == shapes.Plane ? PrimitiveType.Plane*/
                    : PrimitiveType.Cube);
                Mesh mesh2 = tempObj.GetComponent<MeshFilter>().sharedMesh;
                Destroy(tempObj);
                if ((shapes)shape == shapes.Pyramid)
                {
                    tempObj = Instantiate(BundlesManager.pyramidMesh);
                    mesh2 = tempObj.GetComponent<MeshFilter>().sharedMesh;
                    Destroy(tempObj);
                }
                GetComponent<MeshFilter>().mesh = mesh2;

                mesh = Instantiate(GetComponent<MeshFilter>()?.mesh);
                GetComponent<MeshFilter>().mesh = mesh;
            }

        if (customTexture != "")
        {
            newMat = GetSandboxMaterial("Procedural Cube");
            StartCoroutine(ImageGetter.GetTextureFromURL(customTexture, tex =>
            {
                if (tex != null)
                {
                    newMat.mainTexture = tex;
                    ContinueProccess(type, tiling, shape, fixMaterialTiling, customTexture, newMat, renderer);
                }
                else
                {
                    newMat.mainTexture = new();
                    ContinueProccess(type, tiling, shape, fixMaterialTiling, "", newMat, renderer);
                }
            }));
        }
        else
            ContinueProccess(type, tiling, shape, fixMaterialTiling, "", newMat, renderer);
    }

    public void ContinueProccess(materialTypes type, float tiling = 0.25f, shapes? shape = null, bool fixMaterialTiling = false, string customTexture = "", Material newMat = null, Renderer renderer = null)
    {
        if (customTexture != "")
            customTexture = "";
        else if (type == materialTypes.Default)
            newMat = GetSandboxMaterial("Procedural Cube");
        else if (type == materialTypes.Armor)
            newMat = GetSandboxMaterial("Procedural Armor");
        else if (type == materialTypes.Glass)
            newMat = GetSandboxMaterial("Procedural Glass Variant");
        else if (type == materialTypes.Grass)
            newMat = GetPathMaterial("Environment/Layer 1/Grass");
        else if (type == materialTypes.Metal)
            newMat = GetSandboxMaterial("Procedural Metal Cube Variant");
        else if (type == materialTypes.Wood)
            newMat = GetSandboxMaterial("Procedural Wood Cube Variant");
        else if (type == materialTypes.MasterShader)
            newMat = new Material(DefaultReferenceManager.Instance.masterShader);
        else if (type == materialTypes.Brick)
            newMat = GetPathMaterial("uk_construct/ConstructBricks");
        else if (type == materialTypes.TilesDirty)
            newMat = GetPathMaterial("uk_construct/TilesDirty 1");
        else if (type == materialTypes.ConstructBuilding)
            newMat = GetPathMaterial("uk_construct/ConstructBuilding");
        else if (type == materialTypes.BloodPool)
            newMat = GetPathMaterial("Blood/BloodPool");
        else if (type == materialTypes.CyberGrind)
            newMat = GetPathMaterial("Cyber Grind/Virtual");
        else if (type == materialTypes.wip)
            newMat = GetPathMaterial("Dev/wip");
        else if (type == materialTypes.Flesh)
            newMat = GetPathMaterial("Environment/Layer 3/Flesh1");
        else if (type == materialTypes.BrickLight)
            newMat = GetPathMaterial("Environment/Layer 4/BrickLight");
        else if (type == materialTypes.Sand)
            newMat = GetPathMaterial("Environment/Layer 4/SandLarge");
        else if (type == materialTypes.CaveRock)
            newMat = GetPathMaterial("Environment/Layer 5/CaveRock1");
        else if (type == materialTypes.FerryFloor)
            newMat = GetPathMaterial("Environment/Layer 5/FloorPanelBig");
        else if (type == materialTypes.BoneWall)
            newMat = GetPathMaterial("Environment/Layer 6/BoneWall");
        else if (type == materialTypes.WhiteBookshelf)
            newMat = GetPathMaterial("Environment/Layer 7/Bookshelf");
        else if (type == materialTypes.WhiteFlowers)
            newMat = GetPathMaterial("Environment/Layer 7/Flowers");
        else if (type == materialTypes.WarningStripes)
            newMat = GetPathMaterial("Environment/Layer 7/WarningStripes");
        else if (type == materialTypes.WhiteWood)
            newMat = GetPathMaterial("Environment/Layer 7/Wood");
        else if (type == materialTypes.NoCollision)
        {
            newMat = new Material(DefaultReferenceManager.Instance.masterShader);
            GetComponent<Collider>().enabled = false;
        }
        else if (type == materialTypes.UnbreakableGlass)
            newMat = GetPathMaterial("GlassUnbreakable");
        else if (type == materialTypes.LightGlow)
            newMat = GetPathMaterial("LightPillar 3");
        else if (type == materialTypes.Bush)
            newMat = GetPathMaterial("Environment/Layer 1/Bush");
        else if (type == materialTypes.Dirt)
            newMat = GetPathMaterial("Environment/Layer 1/Dirt");
        else if (type == materialTypes.LimboGlass1)
            newMat = GetPathMaterial("Environment/Layer 1/StainedGlassBig");
        else if (type == materialTypes.LimboGlass2)
            newMat = GetPathMaterial("Environment/Layer 1/StainedGlassBlue");
        else if (type == materialTypes.LimboGlass3)
            newMat = GetPathMaterial("Environment/Layer 1/StainedGlassGabriel1");
        else if (type == materialTypes.LimboGlass4)
            newMat = GetPathMaterial("Environment/Layer 1/StainedGlassRed");
        else if (type == materialTypes.StoneMossy)
            newMat = GetPathMaterial("Environment/Layer 1/StoneMossy");
        else if (type == materialTypes.Mulch)
            newMat = GetPathMaterial("Environment/Metal/Mulch");
        else if (type == materialTypes.Metal1)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 1");
        else if (type == materialTypes.Metal2)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 2");
        else if (type == materialTypes.Metal3)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 3");
        else if (type == materialTypes.Metal4)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 4");
        else if (type == materialTypes.Metal5)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 5");
        else if (type == materialTypes.Metal6)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 6");
        else if (type == materialTypes.Metal7)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 7");
        else if (type == materialTypes.Metal8)
            newMat = GetPathMaterial("Environment/Metal/Pattern 1/Metal Pattern 1 8");

        if (newMat == null) return;
        renderer.material = newMat;

        tile = renderer.material.GetTextureScale("_MainTex") * tiling;
        offset = renderer.material.GetTextureOffset("_MainTex") + Vector2.one * (fixMaterialTiling ? 0.5f : 0f);
        mesh = null;
    }

    public void UpdateOffset()
    {
        var renderer = GetComponent<Renderer>();
        if (!renderer) return;

        renderer.material.SetTextureOffset("_MainTex", offset);
    }

    public void Update()
    {
        if (mesh == null)
        {
            mesh = Instantiate(GetComponent<MeshFilter>()?.mesh);
            float maxScale = Mathf.Max(new float[]
            {
                transform.lossyScale.x,
                transform.lossyScale.y,
                transform.lossyScale.z
            });
            float localStep = 1f / (maxScale / 10f);
            localStep = MathF.Max(localStep, 0.1f);
            if (PlayerPrefs.GetInt("PerformanceLighting") == 0 && !EditorManager.Instance.editorOpen)
                SubdivideToUnitSize(mesh, localStep);
            GetComponent<MeshFilter>().mesh = mesh;
            return;
        }
        else
        {
            GetComponent<MeshFilter>().mesh = mesh;
        }

        if (transform.lossyScale != lastScale)
        {
            UpdateUVs();
            lastScale = transform.lossyScale;
        }

        UpdateOffset();
    }

    void UpdateUVs()
    {
        var uvs = new Vector2[mesh.vertices.Length];
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            Vector3 n = mesh.normals[i];

            if (Mathf.Abs(n.y) > 0.9f)
                uvs[i] = new Vector2(v.x * tile.x * transform.lossyScale.x,
                                     v.z * tile.y * transform.lossyScale.z);
            else if (Mathf.Abs(n.x) > 0.9f)
                uvs[i] = new Vector2(v.z * tile.x * transform.lossyScale.z,
                                     v.y * tile.y * transform.lossyScale.y);
            else
                uvs[i] = new Vector2(v.x * tile.x * transform.lossyScale.x,
                                     v.y * tile.y * transform.lossyScale.y);
        }

        mesh.uv = uvs;
    }

    /// <summary> This function took 2 fucking hours to make just for my SSD to die so here's the backup from the exported DLL :3 </summary>
    private void SubdivideToUnitSize(Mesh m, float unit = 1f)
    {
        Vector3[] oldVerts = m.vertices;
        Vector3[] oldNormals = m.normals;
        Vector2[] oldUVs = m.uv;
        int[] oldTris = m.triangles;
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        for (int i = 0; i < oldTris.Length; i += 3)
        {
            int ia = oldTris[i];
            int ib = oldTris[i + 1];
            int ic = oldTris[i + 2];
            Vector3 a = oldVerts[ia];
            Vector3 b = oldVerts[ib];
            Vector3 c = oldVerts[ic];
            Vector3 na = oldNormals[ia];
            Vector3 nb = oldNormals[ib];
            Vector3 nc = oldNormals[ic];
            Vector2 uva = oldUVs[ia];
            Vector2 uvb = oldUVs[ib];
            Vector2 uvc = oldUVs[ic];
            float maxEdge = Mathf.Max(new float[]
            {
                Vector3.Distance(a, b),
                Vector3.Distance(b, c),
                Vector3.Distance(c, a)
            });
            int steps = Mathf.Max(1, Mathf.CeilToInt(maxEdge / unit));
            int[,] indexGrid = new int[steps + 1, steps + 1];
            for (int y = 0; y <= steps; y++)
            {
                for (int x = 0; x <= steps - y; x++)
                {
                    float u = (float)x / (float)steps;
                    float v = (float)y / (float)steps;
                    float w = 1f - u - v;
                    Vector3 pos = a * w + b * u + c * v;
                    Vector3 normal = (na * w + nb * u + nc * v).normalized;
                    Vector2 uv = uva * w + uvb * u + uvc * v;
                    int idx = verts.Count;
                    verts.Add(pos);
                    normals.Add(normal);
                    uvs.Add(uv);
                    indexGrid[x, y] = idx;
                }
            }
            for (int y2 = 0; y2 < steps; y2++)
            {
                for (int x2 = 0; x2 < steps - y2; x2++)
                {
                    int i2 = indexGrid[x2, y2];
                    int i3 = indexGrid[x2 + 1, y2];
                    int i4 = indexGrid[x2, y2 + 1];
                    tris.Add(i2);
                    tris.Add(i3);
                    tris.Add(i4);
                    bool flag = x2 + y2 < steps - 1;
                    if (flag)
                    {
                        int i5 = indexGrid[x2 + 1, y2 + 1];
                        tris.Add(i3);
                        tris.Add(i5);
                        tris.Add(i4);
                    }
                }
            }
        }
        m.Clear();
        m.SetVertices(verts);
        m.SetNormals(normals);
        m.SetUVs(0, uvs);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
    }

    Material GetSandboxMaterial(string path)
    {
        GameObject temporalCube = Instantiate(AddressablesHelper.Ass<GameObject>($"Assets/Prefabs/Sandbox/{path}.prefab"));
        Material mat = new Material(temporalCube.GetComponent<Renderer>().material);

        Destroy(temporalCube);

        return mat;
    }

    Material GetPathMaterial(string path)
    {
        return new Material(AddressablesHelper.Ass<Material>($"Assets/Materials/{path}.mat"));
    }
}