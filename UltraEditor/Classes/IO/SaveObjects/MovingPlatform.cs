using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            spawnedObject?.movingPlatform = obj;
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

        float time = 0;
        public void Tick()
        {
            time += Time.deltaTime;

            if (points == null || points.Length < 2 || affectedCubes == null || affectedCubes.Length == 0)
                return;

            int count = points.Length;
            float total = time * speed / 10;

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
            Quaternion fromRot = points[aIndex].transform.rotation;
            Quaternion toRot = points[bIndex].transform.rotation;

            Vector3 targetPos = Vector3.Lerp(from, to, t);
            Quaternion targetRot = Quaternion.Lerp(fromRot, toRot, t);
            Vector3 delta = targetPos - transform.position;
            Quaternion deltaRot = targetRot * Quaternion.Inverse(transform.rotation);

            transform.position = targetPos;

            foreach (var obj in affectedCubes)
            {
                if (obj != null)
                {
                    obj.transform.position += delta;
                    obj.transform.rotation = deltaRot * obj.transform.rotation;
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

            time = 0;

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

            if (movesWithThePlayer)
            {
                foreach (var obj in affectedCubes)
                {
                    obj.tag = "Moving";
                    obj.AddComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }
}
