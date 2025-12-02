using System;
using System.Collections.Generic;
using System.Text;
using UltraEditor.Classes.Saving;
using UnityEngine;

namespace UltraEditor.Classes
{
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
        }

        public static MaterialChoser Create(GameObject target, materialTypes materialType)
        {
            MaterialChoser obj = target.AddComponent<MaterialChoser>();
            obj.ProcessMaterial(materialType);
            return obj;
        }

        public void ProcessMaterial(materialTypes type)
        {
            Renderer renderer = GetComponent<Renderer>();

            Material newMat = null;

            GetComponent<Collider>().enabled = true;

            if (type == materialTypes.Default)
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

            renderer.material = newMat;

            tile = renderer.material.GetTextureScale("_MainTex") * 0.25f;
            offset = renderer.material.GetTextureOffset("_MainTex");
            mesh = null;
        }

        public void Update()
        {
            if (mesh == null)
            {
                mesh = Instantiate(GetComponent<MeshFilter>()?.mesh);
                GetComponent<MeshFilter>().mesh = mesh;
                return;
            }
            
            if (transform.lossyScale != lastScale)
            {
                UpdateUVs();
                lastScale = transform.lossyScale;
            }
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

        Material GetSandboxMaterial(string path)
        {
            GameObject temporalCube = Instantiate(Plugin.Ass<GameObject>($"Assets/Prefabs/Sandbox/{path}.prefab"));
            Material mat = new Material(temporalCube.GetComponent<Renderer>().material);

            Destroy(temporalCube);

            return mat;
        }

        Material GetPathMaterial(string path)
        {
            return new Material(Plugin.Ass<Material>($"Assets/Materials/{path}.mat"));
        }
    }
}