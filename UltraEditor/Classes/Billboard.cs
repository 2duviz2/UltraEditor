using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.IO.SaveObjects;
using Unity.AI.Navigation;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UltraEditor.Classes
{
    public class Billboard : MonoBehaviour
    {
        public static List<Billboard> billboards = [];
        public static Sprite entitySprite = null;
        public static Sprite activateArena = null;
        public static Sprite activateNextWave = null;
        public static Sprite activateObject = null;
        public static Sprite deathzone = null;
        public static Sprite hudMessageObject = null;
        public static Sprite levelInfoObject = null;
        public static Sprite light = null;
        public static Sprite musicObject = null;
        public static Sprite teleportObject = null;
        public static Sprite checkpoint = null;
        public static Sprite cubeTilingAnimator = null;
        public static Sprite animator = null;
        public static Sprite sfxObject = null;
        public static Sprite skullTrigger = null;
        public static Sprite bookObject = null;

        public static void DeleteAll()
        {
            foreach(var billboard in billboards.ToList())
            {
                if (billboard != null)
                {
                    DestroyImmediate(billboard.gameObject);
                }
            }
            billboards.Clear();
        }

        public static void UpdateBillboards()
        {
            if (entitySprite == null)
            {
                entitySprite = BundlesManager.editorBundle.LoadAsset<Sprite>("entity");
                activateArena = BundlesManager.editorBundle.LoadAsset<Sprite>("ActivateArena");
                activateNextWave = BundlesManager.editorBundle.LoadAsset<Sprite>("ActivateNextWave");
                activateObject = BundlesManager.editorBundle.LoadAsset<Sprite>("ActivateObject");
                deathzone = BundlesManager.editorBundle.LoadAsset<Sprite>("deathzone");
                hudMessageObject = BundlesManager.editorBundle.LoadAsset<Sprite>("hudMessageObject");
                levelInfoObject = BundlesManager.editorBundle.LoadAsset<Sprite>("info");
                light = BundlesManager.editorBundle.LoadAsset<Sprite>("light");
                musicObject = BundlesManager.editorBundle.LoadAsset<Sprite>("music");
                checkpoint = BundlesManager.editorBundle.LoadAsset<Sprite>("checkpoint");
                cubeTilingAnimator = BundlesManager.editorBundle.LoadAsset<Sprite>("cubeTilingAnimator");
                animator = BundlesManager.editorBundle.LoadAsset<Sprite>("animator");
                sfxObject = BundlesManager.editorBundle.LoadAsset<Sprite>("sfxObject");
                skullTrigger = BundlesManager.editorBundle.LoadAsset<Sprite>("skullTrigger");
                bookObject = BundlesManager.editorBundle.LoadAsset<Sprite>("book");
            }

            DeleteAll();

            var allComponents = FindObjectsOfType<Component>(true);

            foreach (var c in allComponents)
            {
                var t = c.transform.position;

                switch (c)
                {
                    case PrefabObject po when po.PrefabAsset.StartsWith("Assets/Prefabs/Enemies/"):
                        NewBillboard(entitySprite, t, c.gameObject);
                        break;

                    case ActivateArena aa:
                        NewBillboard(activateArena, t, c.gameObject);
                        break;

                    case ActivateNextWave nw:
                        NewBillboard(activateNextWave, t, c.gameObject);
                        break;

                    case ActivateObject ao:
                        NewBillboard(activateObject, t, c.gameObject);
                        break;

                    case DeathZone dz when dz.GetComponent<SavableObject>() != null:
                        NewBillboard(deathzone, t, c.gameObject);
                        break;

                    case HUDMessageObject hmo:
                        NewBillboard(hudMessageObject, t, c.gameObject);
                        break;

                    case LevelInfoObject lio:
                        NewBillboard(levelInfoObject, t, c.gameObject);
                        break;

                    case Light l when l.GetComponent<SavableObject>() != null && l.GetComponent<PrefabObject>() == null:
                        NewBillboard(light, t, c.gameObject);
                        break;

                    case MusicObject mo:
                        NewBillboard(musicObject, t, c.gameObject);
                        break;

                    case TeleportObject to:
                        NewBillboard(teleportObject, t, c.gameObject);
                        break;

                    case CheckpointObject co:
                        NewBillboard(checkpoint, t, c.gameObject);
                        break;

                    case CubeTilingAnimator cta:
                        NewBillboard(cubeTilingAnimator, t, c.gameObject);
                        break;

                    case MovingPlatformAnimator mpa:
                        NewBillboard(animator, t, c.gameObject);
                        break;

                    case SFXObject so:
                        NewBillboard(sfxObject, t, c.gameObject);
                        break;

                    case SkullActivatorObject st:
                        NewBillboard(skullTrigger, t, c.gameObject);
                        break;

                    case BookObject bo:
                        NewBillboard(bookObject, t, c.gameObject);
                        break;
                }
            }
        }

        public static List<(Sprite, Material, Vector3)> materials = [];
        public static void NewBillboard(Sprite spr, Vector3 pos, GameObject target)
        {
            GameObject bill = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bill.AddComponent<Billboard>();
            Destroy(bill.GetComponent<Collider>());

            var renderer = bill.GetComponent<Renderer>();

            var shader = Shader.Find("Sprites/Default");
            if (!shader)
            {
                Debug.LogError("Sprites/Default shader not found");
                return;
            }

            (Sprite, Material, Vector3) matt = materials.FirstOrDefault(x => x.Item1 == spr);
            if (matt.Item2 == null)
            {
                var mat = new Material(shader);
                mat.mainTexture = spr.texture;

                Rect r = spr.rect;
                Texture tex = spr.texture;

                mat.mainTextureScale = new Vector2(
                    r.width / tex.width,
                    r.height / tex.height
                );

                mat.mainTextureOffset = new Vector2(
                    r.x / tex.width,
                    r.y / tex.height
                );


                renderer.material = mat;

                float worldX = r.width / spr.pixelsPerUnit;
                float worldY = r.height / spr.pixelsPerUnit;
                bill.transform.localScale = new Vector3(worldX, worldY, 1f);
                materials.Add((spr, mat, bill.transform.localScale));
            }
            else
            {
                renderer.material = matt.Item2;
                bill.transform.localScale = matt.Item3;
            }
            //bill.transform.localScale *= 2f;
            bill.transform.position = pos;
            //bill.transform.position = pos + new Vector3(0, bill.transform.localScale.y / 2f, 0);
            bill.GetComponent<Billboard>().target = target;
            bill.GetComponent<Billboard>().Update();
        }

        public GameObject target;
        public SphereCollider col;

        public void Awake()
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
            billboards.Add(this);
        }

        public void Update()
        {
            if (EditorManager.Instance == null) DeleteAll();
            if (!EditorManager.Instance.editorCanvas.activeInHierarchy) DeleteAll();

            Camera camera = EditorManager.Instance.editorCamera;
            transform.LookAt(camera.transform);

            Renderer renderer = GetComponent<Renderer>();
            Color color = renderer.material.color;
            float alpha = Math.Clamp(Vector3.Distance(camera.transform.position, transform.position) - 1, 0, 5) / (target.activeInHierarchy ? 5f : 20f);
            renderer.material.color = new Color(color.r, color.g, color.b, alpha);

            if (Input.GetMouseButtonDown(0) && EditorManager.Instance.cameraSelector.enabled && EditorManager.Instance.cameraSelector.selectionMode == CameraSelector.SelectionMode.Cursor)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == transform)
                    {
                        EditorManager.Instance.cameraSelector.SelectObject(target);
                        EditorManager.Instance.lastHierarchy = [];
                    }
                }
            }
        }
    }
}