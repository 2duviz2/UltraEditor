using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.TempScripts;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class MovingPlatformAnimator : SavableObject
    {
        public GameObject[] affectedCubes = [];
        public GameObject[] points = [];
        public List<string> pointsIds = [];
        public List<string> affectedCubesIds = [];
        public float speed = 5;
        public bool movesWithThePlayer = true;
        public platformMode mode = platformMode.EaseInOut;

        public enum platformMode
        {
            EaseIn,
            EaseOut,
            EaseInOut,
            Flat,
        }

        public static MovingPlatformAnimator Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            MovingPlatformAnimator obj = target.AddComponent<MovingPlatformAnimator>();
            if (spawnedObject != null) spawnedObject.movingPlatform = obj;
            return obj;
        }

        public void addPointId(string id)
        {
            pointsIds.Add(id);
        }

        public void addAffectedCubeId(string id)
        {
            affectedCubesIds.Add(id);
        }

        public void Tick()
        {
            if (points == null || points.Length < 2 || affectedCubes == null || affectedCubes.Length == 0)
                return;

            int count = points.Length;
            float total = Time.time * speed / 10;

            int aIndex = Mathf.FloorToInt(total) % count;
            int bIndex = (aIndex + 1) % count;

            float t = total - Mathf.Floor(total);

            switch (mode)
            {
                case platformMode.EaseIn:
                    t = t * t;
                    break;

                case platformMode.EaseOut:
                    t = 1f - Mathf.Pow(1f - t, 2f);
                    break;

                case platformMode.EaseInOut:
                    t = t < 0.5f
                        ? 2f * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                    break;

                case platformMode.Flat:
                    break;
            }

            Vector3 from = points[aIndex].transform.position;
            Vector3 to = points[bIndex].transform.position;

            Vector3 targetPos = Vector3.Lerp(from, to, t);
            Vector3 delta = targetPos - transform.position;

            transform.position = targetPos;

            foreach (var obj in affectedCubes)
            {
                if (obj != null)
                {
                    obj.transform.position += delta;
                    if (obj.GetComponent<MoveWithPlayer>() != null)
                        obj.GetComponent<MoveWithPlayer>().delta = delta;
                }
            }
        }

        public void createAnimator()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            affectedCubes = [];
            points = [];

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
            foreach (var e in pointsIds)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        List<GameObject> objs = points.ToList();
                        objs.Add(obj.gameObject);
                        points = objs.ToArray();
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            List<GameObject> objs = points.ToList();
                            objs.Add(obj.gameObject);
                            points = objs.ToArray();
                            found = true;
                            break;
                        }
                    }
            }

            foreach (var obj in affectedCubes)
            {
                if (movesWithThePlayer)
                {
                    obj.AddComponent<MoveWithPlayer>();
                    obj.AddComponent<Rigidbody>();
                    obj.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }
}
