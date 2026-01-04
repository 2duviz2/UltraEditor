using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class CubeTilingAnimator : SavableObject
    {
        public Vector2 scrolling = Vector2.one;
        public GameObject[] affectedCubes = [];
        public List<string> affectedCubesIds = [];

        public static CubeTilingAnimator Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            CubeTilingAnimator obj = target.AddComponent<CubeTilingAnimator>();
            if (spawnedObject != null) spawnedObject.cubeTilingAnimator = obj;
            return obj;
        }

        public void addId(string id)
        {
            affectedCubesIds.Add(id);
        }

        public void createAnimator()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            foreach (var e in affectedCubesIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> objs = affectedCubes.ToList();
                        objs.Add(obj.gameObject);
                        affectedCubes = objs.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> objs = affectedCubes.ToList();
                            objs.Add(obj.gameObject);
                            affectedCubes = objs.ToArray();
                            found = true;
                            break;
                        }
                    }
            }
        }

        public void Tick()
        {
            foreach (var c in affectedCubes)
            {
                if (c == null) continue;
                MaterialChoser mc = c.GetComponent<MaterialChoser>();

                if (mc != null)
                {
                    mc.offset += scrolling * Time.unscaledDeltaTime;
                }
            }
        }
    }
}
