using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace UltraEditor.Classes
{
    internal class EditorVisualizers
    {
        public static void RebuildNavMeshVis(NavMeshSurface surface)
        {
            if (surface == null)
            {
                Debug.LogError("RebuildNavMeshVis: Surface is null!");
                return;
            }
            NavMeshTriangulation navMeshTris = NavMesh.CalculateTriangulation();
            if (navMeshTris.vertices == null || navMeshTris.vertices.Length == 0)
            {
                Debug.LogWarning("RebuildNavMeshVis: No NavMesh data found, nothing to visualize.");
                return;
            }

            Transform parent = surface.transform;
            Transform visTransform = parent.Find("NavMeshVis");
            GameObject obj;

            if (visTransform == null)
            {
                obj = new GameObject("NavMeshVis");
                obj.transform.SetParent(parent);
                obj.transform.localPosition = new Vector3(0f, 1f, 0f);
                obj.layer = LayerMask.NameToLayer("Invisible");
            }
            else
            {
                obj = visTransform.gameObject;
            }

            MeshFilter meshFilter = obj.GetComponent<MeshFilter>() ?? obj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>() ?? obj.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = CreateNewMesh(MeshTopology.Triangles, navMeshTris.vertices, navMeshTris.indices);

            Material mat = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.25f, 0.75f, 1f, 0.3f)
            };
            meshRenderer.sharedMaterial = mat;

            Plugin.LogInfo("NavMesh visualization built successfully");
        }

        public static Mesh CreateNewMesh(MeshTopology topology, Vector3[] vertices, int[] indices)
        {
            Mesh mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, topology, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}