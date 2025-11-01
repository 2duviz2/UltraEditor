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

        public enum materialTypes
        {
            Default,
            MasterShader
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

            if (type == materialTypes.Default)
                //renderer.material = Material.GetDefaultMaterial();
                renderer.material = GetDefaultMaterial();
            if (type == materialTypes.MasterShader)
                renderer.material = new Material(DefaultReferenceManager.Instance.masterShader);

            tile = renderer.material.GetTextureScale("_MainTex") * 0.1f;
            offset = renderer.material.GetTextureOffset("_MainTex");
        }

        public void Update()
        {
            Renderer renderer = GetComponent<Renderer>();
            Vector3 scale = transform.lossyScale;

            if (scale.x == 0 || scale.y == 0 || scale.z == 0)
                return;

            float smallest = Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z));
            Vector3 normalized = new Vector3(scale.x / smallest, scale.y / smallest, scale.z / smallest);

            Vector2 tiling = new Vector2(tile.x * normalized.x, tile.y * normalized.y);

            renderer.material.SetTextureScale("_MainTex", tiling);
        }

        Material GetDefaultMaterial()
        {
            GameObject temporalCube = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Sandbox/Procedural Cube.prefab"));
            Material mat = temporalCube.GetComponent<Renderer>().material;

            Destroy(temporalCube);

            return mat;
        }
    }
}