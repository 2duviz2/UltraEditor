using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UltraEditor.Classes.Canvas;
using UltraEditor.Classes.Editor;
using UltraEditor.Classes.IO;
using UltraEditor.Classes.IO.SaveObjects;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Component = UnityEngine.Component;
using NewTeleportObject = UltraEditor.Classes.IO.SaveObjects.TeleportObject;

namespace UltraEditor.Classes
{
    public class EditorManager : MonoBehaviour
    {
        public static EditorManager Instance;

        public GameObject editorCanvas;
        public Camera editorCamera;
        public CameraSelector cameraSelector;
        public GameObject blocker;

        bool editorOpen = false;
        bool destroyedLastFrame = false;
        static public bool advancedInspector = false;
        static public bool friendlyAdvancedInspector = false;
        public static bool logShit = false;
        public static bool canOpenEditor = false;

        static string tempScene = ExampleScenes.GetDefaultScene();

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Update()
        {
            if (editorCanvas.activeSelf)
            {
                UpdateHierarchy();
            }
            
            if (destroyedLastFrame)
            {
                destroyedLastFrame = false;
                Billboard.UpdateBillboards();
            }

            if (editorOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                if (!cameraSelector.dragging && !editorCamera.GetComponent<CameraMovement>().moving())
                    Cursor.visible = true;
            }

            if (Input.GetKeyDown(Plugin.toggleEditorCanvasKey))
            {
                editorCanvas.SetActive(!editorCanvas.activeSelf);
                cameraSelector.ClearHover();
            }

            if (Input.GetKeyDown(Plugin.deleteObjectKey) && editorCanvas.activeSelf)
            {
                if (Input.GetKey(Plugin.ctrlKey) && Input.GetKey(Plugin.shiftKey) && friendlyAdvancedInspector)
                    DeleteScene(true);
                else if (IsObjectEditable())
                    deleteObject();
            }

            if (Plugin.isToggleEnabledKeyPressed() && IsObjectEditable() && editorCanvas.activeSelf)
                toggleObject();

            if (Plugin.isDuplicateKeyPressed() && IsObjectEditable() && editorCanvas.activeSelf)
                duplicateObject();

            if (Input.GetKey(Plugin.createCubeKey) && editorCanvas.activeSelf)
            {
                createCube(true, false);
            }

            cameraSelector.enabled = (!blocker.activeSelf || cameraSelector.dragging) && editorCamera.gameObject.activeSelf;

            if (Input.GetMouseButtonUp(0))
            {
                if (logShit)
                Plugin.LogInfo($"Released mouse button with " +
                    $"{(holdingObject ? holdingObject.name : "null")} & " +
                    $"{(holdingTarget ? holdingTarget.name : "null")}");

                if (holdingObject != null && holdingTarget != null && holdingObject != holdingTarget)
                {
                    if (IsObjectEditable(holdingTarget) || advancedInspector)
                    {
                        if (logShit)
                            Plugin.LogInfo($"Dropped object: {holdingObject.name} into target: {holdingTarget.name}");
                        holdingObject.transform.SetParent(holdingTarget.transform);
                        cameraSelector.selectedObject = holdingTarget;
                        lastSelected = null;
                        UpdateHierarchy();
                        holdingObject = null;
                        holdingTarget = null;
                    }
                }

                else if (holdingObject != holdingTarget && holdingObject != null)
                {
                    if (logShit)
                        Plugin.LogInfo($"Released object: {holdingObject.name} from target");
                    holdingObject.transform.SetParent(null);
                    cameraSelector.selectedObject = holdingTarget;
                    lastSelected = null;
                    UpdateHierarchy();
                    holdingObject = null;
                    holdingTarget = null;
                }
            }

            if (Plugin.isSelectPressed() && cameraSelector.selectedObject != null && editorCanvas.activeSelf)
            {
                SelectObject(cameraSelector.selectedObject);
            }

            if (FinalRank.Instance != null)
                FinalRank.Instance.targetLevelName = "Main Menu";
        }

        public void LateUpdate()
        {
            if (editorOpen)
                cameraSelector.RenderInsides();
        }

        public static TMP_Text MissionNameText = null;
        public static string EditorSceneName = "UltraEditor";
        static NavMeshSurface navMeshSurface;
        public static void DeleteScene(bool force = false)
        {
            if ((force || (SceneHelper.CurrentScene == EditorSceneName && !StatsManager.Instance.timer)))
            {
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList())
                {
                    if (obj == null) continue;
                    if (logShit)
                        Plugin.LogInfo($"Trying to detroy {obj.name}");
                    if (obj.GetComponent<SavableObject>() != null || obj.GetComponent<SpawnedObject>() != null || obj.name == "Automated Gore Zone")
                    {
                        if (logShit)
                            Plugin.LogInfo($"Destroyed {obj.name}");
                        Destroy(obj);
                    }
                }
                Billboard.DeleteAll();
            }
        }

        public static void Create()
        {
            if (Instance == null)
            {
                DeleteScene();
                GameObject obj = new GameObject("EditorManager");
                Instance = obj.AddComponent<EditorManager>();
                Instance.CreateUI();
            }
            else
            {
                Instance.CreateUI();
            }
        }

        int defaultCullingMask = 0;
        public void CreateUI()
        {
            if (editorCanvas != null)
            {
                LoadingHelper.cachedIds.Clear();
                editorOpen = !editorOpen;
                editorCamera.gameObject.SetActive(editorOpen);
                if (NewMovement.Instance != null)
                    NewMovement.Instance.gameObject.SetActive(!editorOpen);
                editorCanvas.SetActive(editorOpen);
                blocker.SetActive(true);
                cameraSelector.ClearHover();
                cameraSelector.UnselectObject();
                cameraSelector.selectionMode = CameraSelector.SelectionMode.Cursor;
                Billboard.DeleteAll();

                if (!editorOpen)
                {
                    if (SceneHelper.CurrentScene != "Main Menu")
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    if (Input.GetKey(Plugin.shiftKey))
                    {
                        NewMovement.Instance.transform.position = editorCamera.transform.position;
                        NewMovement.Instance.transform.rotation = editorCamera.transform.rotation;
                    }


                    foreach (var item in FindObjectsOfType<Door>())
                    {
                        var m = item.GetType().GetMethod("GetPos",
                            BindingFlags.Instance | BindingFlags.NonPublic);

                        m.Invoke(item, null);
                    }

                    RebuildNavmesh(false);
                }
                else
                {
                    editorCamera.transform.position = NewMovement.Instance.transform.position;
                    editorCamera.transform.rotation = NewMovement.Instance.transform.rotation;
                }

                if (editorOpen && !string.IsNullOrEmpty(tempScene) && !advancedInspector && SceneHelper.CurrentScene == EditorSceneName && canOpenEditor)
                {
                    StartCoroutine(GoToBackupScene());
                }
                if (!editorOpen && !advancedInspector && SceneHelper.CurrentScene == EditorSceneName && canOpenEditor)
                {
                    tempScene = GetSceneJson();

                    string path = Application.persistentDataPath + $"/ULTRAEDITOR/backups";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.WriteAllText(path + $"/{DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss")}.uterus", tempScene);
                }

                Time.timeScale = editorOpen ? 0f : 1f;
                DisableAlert();

                return;
            }

            GameObject prefab = BundlesManager.editorBundle.LoadAsset<GameObject>("EditorCanvas");

            if (prefab == null)
            {
                Plugin.LogError("Prefab 'EditorCanvas' not found in AssetBundle");
                return;
            }

            editorOpen = true;

            editorCanvas = Instantiate(prefab);
            editorCanvas.name = "EditorCanvas_Instance";
            Plugin.LogInfo("EditorCanvas instantiated successfully!");

            GameObject cameraObj = new GameObject("EditorCamera");
            editorCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.AddComponent<CameraMovement>();
            cameraObj.AddComponent<CameraSelector>();
            editorCamera.transform.position = Camera.main.transform.position;
            editorCamera.depth = 100;
            editorCamera.fieldOfView = 105;
            cameraSelector = cameraObj.GetComponent<CameraSelector>();
            blocker = editorCanvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

            if (NewMovement.Instance != null)
            {
                NewMovement.Instance.gameObject.SetActive(!editorOpen);
                defaultCullingMask = CameraController.Instance.cam.cullingMask;
                ChangeCameraCullingLayers(defaultCullingMask);
                NewMovement.Instance.endlessMode = false;
            }
            Time.timeScale = editorOpen ? 0f : 1f;

            SetupButtons();
            if (FindObjectOfType<NavMeshSurface>() != null)
                EditorVisualizers.RebuildNavMeshVis(FindObjectOfType<NavMeshSurface>());

            if (!string.IsNullOrEmpty(tempScene) && !advancedInspector && SceneHelper.CurrentScene == EditorSceneName && canOpenEditor) // load backup level after restart
            {
                StartCoroutine(GoToBackupScene());
            }
        }

        void SetupButtons()
        {
            Transform buttons = editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1);
            Transform fileB = buttons.GetChild(0).GetChild(3);
            Transform editB = buttons.GetChild(1).GetChild(3);
            Transform addB = buttons.GetChild(2).GetChild(3);
            Transform viewB = buttons.GetChild(3).GetChild(3);
            Transform helpB = buttons.GetChild(4).GetChild(3);
            // File
            fileB.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                TryToLoadShit();
            });

            fileB.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                TryToSaveShit();
            });

            fileB.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                string path = $"{Application.persistentDataPath}/ULTRAEDITOR";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                Application.OpenURL($"file://{path}");
            });

            fileB.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                DeleteScene(true);
                SetAlert("Scene deleted!", "Info!", new Color(1, 0.5f, 0.25f));
            });

            fileB.GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                StartCoroutine(GoToBackupScene());
            });

            fileB.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
            {
                SetAlert("Couldn't copy scene!");
                GUIUtility.systemCopyBuffer = GetSceneJson();
                SetAlert("Scene copied!", "Info!", col: new Color(0.25f, 1f, 0.25f));
            });

            // Edit
            editB.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                toggleObject();
            });

            editB.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                deleteObject();
            });

            editB.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                duplicateObject();
            });

            editB.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                RebuildNavmesh(true);
            });

            // Add
            addB.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube();
            });

            addB.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube(true);
            });

            addB.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube(true, false);
            });

            addB.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                createFloor(new Vector3(25, 1, 25));
            });

            addB.GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                createFloor(new Vector3(25, 10, 1));
            });

            addB.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube(pos : new Vector3(0.00f, 90.00f, 4.25f), layer : "Invisible", objName : "Invisible cube", matType : MaterialChoser.materialTypes.NoCollision);
            });

            addB.GetChild(7).GetComponent<Button>().onClick.AddListener(() =>
            {
                TryToGroupName();
            });

            // View
            viewB.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeCameraCullingLayers(-1);
            });

            viewB.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeCameraCullingLayers(defaultCullingMask);
            });

            viewB.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                friendlyAdvancedInspector = true;
                SetAlert("Advanced inspector will enable more options and is meant for people who have gotten used to the editor.", "Warning!");
                UpdateInspector();
                lastHierarchy = [];
            });

            viewB.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                friendlyAdvancedInspector = false;
                UpdateInspector();
                lastHierarchy = [];
            });

            viewB.GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeLighting(0);
            });

            viewB.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeLighting(1);
            });
        }

        void RebuildNavmesh(bool forceFindNavmesh)
        {
            if (navMeshSurface == null)
                navMeshSurface = FindObjectOfType<NavMeshSurface>();

            if (navMeshSurface == null)
            {
                Log("Creating navmesh...");
                GameObject navMeshObj = new("NavMeshSurface");
                navMeshSurface = navMeshObj.AddComponent<NavMeshSurface>();
                navMeshSurface.collectObjects = CollectObjects.All;
                navMeshSurface.BuildNavMesh();
                Log("NavMeshSurface created and built.");
            }

            else if (navMeshSurface != null)
            {
                Log("Building navmesh...");
                navMeshSurface.BuildNavMesh();
                EditorVisualizers.RebuildNavMeshVis(navMeshSurface);
                Log("NavMesh rebuilt.");
            }
            else
            {
                Log("No NavMeshSurface found to rebuild.");
            }
        }

        public GameObject SpawnAsset(string dir, bool isLoading = false, bool createPrefabObject = true)
        {
            string realPath = dir;
            if (dir == "DuvizPlush") dir = "Assets/Prefabs/Fishing/Fish Pickup Template.prefab";
            if (dir == "DuvizPlushFixed") dir = "Assets/Prefabs/Fishing/Fish Pickup Template.prefab";
            GameObject obj = null;

            // Exclusive exceptions
            if (dir == "ImCloudingIt")
            {
                obj = Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("Cloud"));
            }
            else if (dir == "AltarBlueOff")
            {
                obj = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Levels/Interactive/Altar (Blue).prefab"));

                //            |Cube       |SkullBlue
                obj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                obj.name += " (Off)";
            }
            else if (dir == "AltarRedOff")
            {
                obj = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Levels/Interactive/Altar (Red).prefab"));

                //            |Cube       |SkullBlue
                obj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                obj.name += " (Off)";
            }
            else if (dir == "Assets/Prefabs/Levels/Decorations/SuicideTreeHungry.prefab(Active)")
            {
                obj = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Levels/Decorations/SuicideTreeHungry.prefab"));

                obj.transform.GetComponent<DisabledEnemiesChecker>().enabled = false;
                //            |SuicideTree4
                obj.transform.GetChild(0).GetComponent<BloodFiller>().enabled = false;
                //            |FinishEffects
                obj.transform.GetChild(1).gameObject.SetActive(true);
                //            |BloodLeaves
                obj.transform.GetChild(2).gameObject.SetActive(true);
                obj.name += " (Active)";
            }
            else
            {
                // Spawn the object like normal
                obj = Instantiate(Plugin.Ass<GameObject>(dir));
            }

            obj.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;
            if (createPrefabObject)
            {
                PrefabObject.Create(obj, realPath);
            }
            if (Input.GetKey(Plugin.shiftKey) && cameraSelector.selectedObject != null)
            {
                obj.transform.SetParent(cameraSelector.selectedObject.transform);
                lastSelected = null;
            }
            else
            {
                cameraSelector.SelectObject(obj);
            }

            if (Input.GetKey(Plugin.altKey))
            {
                obj.SetActive(false);
            }
            else
            {
                if (dir.StartsWith("Assets/Prefabs/Enemies/") && !isLoading)
                {
                    obj.SetActive(false);
                    SetAlert("Enemies will always spawn disabled in the editor, they must be enabled with ActivateArena objects.", title: "Warning!");
                }
            }

            // Manage special stuff
            if (dir == "Assets/Prefabs/Levels/Checkpoint.prefab" && !isLoading)
                SetAlert("You need to assign at least one item in rooms for the checkpoint to work.", "Warning!");
            if (dir == "Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab" && !isLoading)
                SetAlert("FinalDoor/FinalDoorOpener must be activated to open the door, it must be activated with a trigger and in this version completing the level will result in an infinite stats screen.", "Warning!");

            if (dir == "Bonus" || dir == "Assets/Prefabs/Levels/BonusDualWield Variant.prefab" || dir == "Assets/Prefabs/Levels/BonusSuperCharge.prefab")
                obj.GetComponent<Bonus>().secretNumber = 100000;

            // Manage the blahaj and plushies
            if (dir == "Assets/Prefabs/Fishing/Fish Pickup Template.prefab")
            {
                GameObject blahaj = null;
                if (realPath == "Assets/Prefabs/Fishing/Fish Pickup Template.prefab")
                    blahaj = SpawnAsset("Assets/Prefabs/Fishing/Fishes/Shark Fish.prefab", false, false);
                else if (realPath == "DuvizPlush")
                    blahaj = Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("DuvizPlush"));
                else if (realPath == "DuvizPlushFixed")
                    blahaj = Instantiate(BundlesManager.editorBundle.LoadAsset<GameObject>("DuvizPlushFixed"));
                obj.transform.localEulerAngles = Vector3.zero;
                blahaj.transform.SetParent(obj.transform);
                blahaj.transform.localPosition = Vector3.zero;
                blahaj.transform.localEulerAngles = Vector3.zero;
                if (dir == realPath)
                    obj.name = "Blahaj";
                else
                    obj.name = realPath;
            }

            if (!isLoading)
            {
                PlayAudio(spawnAsset);
                Billboard.UpdateBillboards();
            }

            return obj;
        }

        void duplicateObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                cameraSelector.ClearSelectedMaterial();
                GameObject newObj = Instantiate(cameraSelector.selectedObject);

                newObj.name = newObj.name.Replace("(Clone)", "");

                if (cameraSelector.selectedObject.transform.parent != null)
                    newObj.transform.SetParent(cameraSelector.selectedObject.transform.parent);
                    newObj.transform.SetParent(cameraSelector.selectedObject.transform.parent);

                newObj.transform.localScale = cameraSelector.selectedObject.transform.localScale;
                newObj.transform.position = cameraSelector.selectedObject.transform.position;
                newObj.transform.rotation = cameraSelector.selectedObject.transform.rotation;

                cameraSelector.SelectObject(newObj);

                if (Input.GetKey(Plugin.altKey)) newObj.SetActive(false);
            }
        }

        void deleteObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                GameObject toDestroy = cameraSelector.selectedObject;
                GameObject toParent = null;
                if (cameraSelector.selectedObject.transform.parent != null)
                    toParent = cameraSelector.selectedObject.transform.parent.gameObject;
                else
                    cameraSelector.UnselectObject();
                Destroy(toDestroy);
                if (toParent != null)
                    cameraSelector.SelectObject(toParent);
                destroyedLastFrame = true;
                PlayAudio(destroyObject);
            }
        }

        void toggleObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                lastHierarchy = new GameObject[0];
                cameraSelector.selectedObject.SetActive(!cameraSelector.selectedObject.activeSelf);
                PlayAudio(cameraSelector.selectedObject.activeSelf ? activateObject : inactivateObject);
            }
        }

        public GameObject createCube(bool createRigidbody = false, bool useGravity = true, Vector3? pos = null, string layer = "Default", string objName = "Cube", MaterialChoser.materialTypes matType = MaterialChoser.materialTypes.MasterShader)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objName;
            cube.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;
            if (pos != null)
                cube.transform.position = (Vector3)pos;
            cube.layer = LayerMask.NameToLayer(layer);
            cube.transform.localScale = Vector3.one;
            CubeObject.Create(cube, matType);

            if (createRigidbody)
                cube.AddComponent<Rigidbody>().useGravity = useGravity;
            cube.GetComponent<Renderer>().material = new Material(DefaultReferenceManager.Instance.masterShader);

            if (Input.GetKey(Plugin.shiftKey) && cameraSelector.selectedObject != null)
            {
                cube.transform.SetParent(cameraSelector.selectedObject.transform);
                lastSelected = null;
            }
            else
                cameraSelector.SelectObject(cube);

            if (Input.GetKey(Plugin.altKey)) cube.SetActive(false);

            return cube;
        }

        void createFloor(Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f + Vector3.down * 1f;
            cube.transform.localScale = scale;
            cube.layer = LayerMask.NameToLayer("Outdoors");
            cube.tag = "Floor";
            cube.name = scale.y > 1 ? "Wall" : "Floor";

            CubeObject.Create(cube, MaterialChoser.materialTypes.Default);

            if (Input.GetKey(Plugin.shiftKey) && cameraSelector.selectedObject != null)
            {
                cube.transform.SetParent(cameraSelector.selectedObject.transform);
                lastSelected = null;
            }
            else
                cameraSelector.SelectObject(cube);
        }

        void ChangeCameraCullingLayers(int layerMask)
        {
            editorCamera.cullingMask = layerMask;
        }

        void ChangeLighting(int lit)
        {
            if (lit == 1)
                editorCamera.GetComponent<CameraMovement>().setUnlit(false);
            else if (lit == 0)
                editorCamera.GetComponent<CameraMovement>().setUnlit(true);
        }

        string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "";

            var parts = new System.Collections.Generic.List<string>();
            Transform t = go.transform;
            while (t != null)
            {
                if (!string.IsNullOrEmpty(t.name))
                    parts.Add(t.name);
                t = t.parent;
            }

            parts.Reverse();
            return "/" + string.Join("/", parts);
        }

        Component[] lastComponents = [];
        public GameObject[] lastHierarchy = [];
        GameObject lastSelected = null;
        GameObject holdingObject = null;
        GameObject holdingTarget = null;
        void UpdateHierarchy()
        {
            if (editorCanvas == null) return;

            GameObject content = editorCanvas.transform.GetChild(0).GetChild(2).GetChild(3).gameObject;
            GameObject templateItem = content.transform.GetChild(0).gameObject;
            templateItem.SetActive(false);

            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            if (lastHierarchy.Length == rootObjects.Length)
            {
                bool same = true;
                for (int i = 0; i < lastHierarchy.Length; i++)
                {
                    if (lastHierarchy[i] != rootObjects[i] && rootObjects[i].GetComponent<Billboard>() == null)
                    {
                        same = false;
                        break;
                    }
                }

                bool sameComponents = true;

                if (cameraSelector.selectedObject != null)
                {
                    var currentComponents = cameraSelector.selectedObject.GetComponents<Component>();
                    sameComponents = lastComponents.Length == currentComponents.Length &&
                                lastComponents.SequenceEqual(currentComponents);
                }
                if (same && lastSelected == cameraSelector.selectedObject && sameComponents) return; // No change in hierarchy
            }

            lastHierarchy = rootObjects;
            lastSelected = cameraSelector.selectedObject;
            holdingObject = null;

            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = content.transform.GetChild(i);
                if (child.gameObject != templateItem)
                {
                    Destroy(child.gameObject);
                }
            }

            GameObject[] objectsToHierarch = rootObjects;

            if (cameraSelector.selectedObject != null)
            {
                string path = GetHierarchyPath(cameraSelector.selectedObject);

                CreateHierarchyItem(null, $"< {cameraSelector.selectedObject.name}", path, true, new Color(0.5f, 1, 0.5f, cameraSelector.selectedObject.activeSelf ? 1 : 0.5f));

                List<GameObject> objectsToHierarchList = new List<GameObject>();

                foreach (Transform obj in cameraSelector.selectedObject.GetComponentInChildren<Transform>())
                {
                    if (obj.parent == cameraSelector.selectedObject.transform)
                    {
                        objectsToHierarchList.Add(obj.gameObject);
                    }
                }

                objectsToHierarch = objectsToHierarchList.ToArray();
            }

            foreach (GameObject obj in objectsToHierarch)
            {
                if (cameraSelector.selectedObject == null && SceneHelper.CurrentScene == EditorSceneName)
                {
                    if (obj.GetComponent<SavableObject>() == null && !advancedInspector)
                        continue;
                }

                if (obj == editorCamera.gameObject || obj == this.gameObject || obj == editorCanvas.gameObject) continue;

                CreateHierarchyItem(obj);
            }

            UpdateInspector();
            Billboard.UpdateBillboards();
        }

        void InitializeDefaultFields(Component component)
        {
            var fields = component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

            foreach (var field in fields)
            {
                var value = field.GetValue(component);
                if (value != null) continue;

                var fieldType = field.FieldType;

                if (fieldType == typeof(string))
                {
                    field.SetValue(component, "");
                }
                else if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    try
                    {
                        var list = Activator.CreateInstance(fieldType);
                        field.SetValue(component, list);
                    }
                    catch { }
                }
                else if (fieldType.IsArray)
                {
                    var arr = Array.CreateInstance(fieldType.GetElementType(), 0);
                    field.SetValue(component, arr);
                }
                else if (fieldType.IsValueType)
                {
                    field.SetValue(component, Activator.CreateInstance(fieldType));
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                {
                    
                }
                else
                {
                    try
                    {
                        field.SetValue(component, Activator.CreateInstance(fieldType));
                    }
                    catch { }
                }
            }
        }

        public bool IsObjectEditable(GameObject obj = null)
        {
            if (obj != null)
                return (obj.GetComponent<SavableObject>() != null || advancedInspector);
            if (advancedInspector) return true;
            if (cameraSelector.selectedObject == null) return false;
            return cameraSelector.selectedObject.GetComponent<SavableObject>() != null;
        }

        string lastFieldText = "";
        Enum lastEnum = null;
        Type lastEnumType = null;
        object coppiedValue = null;
        Type lastCoppiedType = null;
        List<(string, Type, float)> searchResults = new List<(string, Type, float)>();
        static List<(string, float)> sceneResults = new List<(string, float)>();
        Type arrayType = null;
        public void UpdateInspector()
        {
            GameObject content = editorCanvas.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(3).gameObject;
            GameObject templateItem = content.transform.GetChild(0).gameObject;
            templateItem.SetActive(false);

            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = content.transform.GetChild(i);
                if (child.gameObject != templateItem)
                {
                    Destroy(child.gameObject);
                }
            }

            if (cameraSelector.selectedObject != null)
            {
                if (!IsObjectEditable())
                {
                    lastComponents = cameraSelector.selectedObject.GetComponents<Component>();
                    return;
                }
                if (advancedInspector || (cameraSelector.selectedObject.GetComponent<ActivateArena>() == null && cameraSelector.selectedObject.GetComponent<ActivateNextWave>() == null && cameraSelector.selectedObject.GetComponent<ActivateObject>() == null && cameraSelector.selectedObject.GetComponent<DeathZone>() == null && cameraSelector.selectedObject.GetComponent<HUDMessageObject>() == null && cameraSelector.selectedObject.GetComponent<NewTeleportObject>() == null && cameraSelector.selectedObject.GetComponent<LevelInfoObject>() == null && cameraSelector.selectedObject.GetComponent<Light>() == null && cameraSelector.selectedObject.GetComponent<PrefabObject>() == null))
                {
                    CreateInspectorItem("Add component", InspectorItemType.Button, "Add").AddListener(() =>
                    {
                        GameObject addComponentPopup = editorCanvas.transform.GetChild(0).GetChild(6).gameObject;
                        TMP_InputField field = addComponentPopup.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>();
                        TMP_Text foundComponents = addComponentPopup.transform.GetChild(2).GetComponent<TMP_Text>();
                        Button addButton = addComponentPopup.transform.GetChild(4).GetComponent<Button>();

                        addComponentPopup.SetActive(true);

                        field.Select();

                        field.onValueChanged.RemoveAllListeners();
                        addButton.onClick.RemoveAllListeners();
                        field.onValueChanged.AddListener((string val) =>
                        {
                            searchResults.Clear();

                            var monoTypes = EditorComponentsList.GetMonoBehaviourTypes();

                            foreach (var type in monoTypes)
                            {
                                float accuracy = 0f;
                                string typeName = type.Name.ToLower();
                                string searchName = val.ToLower();
                                int minLength = Math.Min(typeName.Length, searchName.Length);
                                for (int i = 0; i < minLength; i++)
                                {
                                    if (typeName[i] == searchName[i])
                                        accuracy += 1f / minLength;
                                    else
                                        break;
                                }
                                if (typeName.Contains(searchName))
                                    accuracy += 0.5f;

                                if (typeName == searchName)
                                    accuracy += 10000f;

                                if (accuracy > 0f)
                                    searchResults.Add((type.Name, type, accuracy));
                            }

                            searchResults = searchResults.OrderByDescending(t => t.Item3).ToList();

                            foundComponents.text = "Found component:\n";
                            foreach (var result in searchResults.Take(3))
                            {
                                foundComponents.text += $"{result.Item1}<color=grey>   ";
                            }
                        });

                        addButton.onClick.AddListener(() =>
                        {
                            addComponentPopup.SetActive(false);
                            if (cameraSelector.selectedObject == null) return;
                            if (searchResults.Count > 0)
                            {
                                string componentName = searchResults[0].Item1;
                                Type componentType = searchResults[0].Item2;
                                if (componentType != null && typeof(Component).IsAssignableFrom(componentType))
                                {
                                    Component c = cameraSelector.selectedObject.AddComponent(componentType);
                                    InitializeDefaultFields(c);

                                    if (EditorComponentsList.IsTrigger(c))
                                    {
                                        if (cameraSelector.selectedObject.GetComponent<Collider>() != null)
                                        {
                                            cameraSelector.selectedObject.GetComponent<Collider>().isTrigger = true;
                                            cameraSelector.selectedObject.layer = LayerMask.NameToLayer("Invisible");
                                            SetAlert("Collider has been set to be a trigger and layer to Invisible.", "Info!", new Color(1, 0.5f, 0.25f));
                                            if (c is SavableObject)
                                                Destroy(cameraSelector.selectedObject.GetComponent<CubeObject>());
                                        }
                                    }

                                    // Exclusive exceptions for default components
                                    if (c is ActivateArena arena)
                                    {
                                        arena.doors = [];
                                        arena.onlyWave = true;
                                    }
                                    else if (c is ActivateNextWave nextWave)
                                    {
                                        nextWave.noActivationDelay = true;
                                    }
                                    else if (c is HudMessage hudMessage)
                                    {
                                        hudMessage.timed = true;
                                    }

                                    UpdateInspector();
                                    PlayAudio(addComponent);
                                    Billboard.UpdateBillboards();
                                }
                                else
                                {
                                    Plugin.LogError($"Component type '{componentName}' not found.");
                                }
                            }
                        });
                    });
                }

                CreateInspectorItem("Name", InspectorItemType.InputField, cameraSelector.selectedObject.name).AddListener(() =>
                {
                    cameraSelector.selectedObject.name = lastFieldText;
                    UpdateInspector();
                    lastSelected = null;
                });

                CreateInspectorItem("Tag", InspectorItemType.InputField, cameraSelector.selectedObject.tag).AddListener(() =>
                {
                    cameraSelector.selectedObject.tag = lastFieldText;
                    UpdateInspector();
                });

                CreateInspectorItem("Layer", InspectorItemType.InputField, LayerMask.LayerToName(cameraSelector.selectedObject.layer)).AddListener(() =>
                {
                    if (LayerMask.NameToLayer(lastFieldText) == -1)
                    {
                        UpdateInspector();
                        return;
                    }
                    cameraSelector.selectedObject.layer = LayerMask.NameToLayer(lastFieldText);
                    UpdateInspector();
                });

                lastComponents = cameraSelector.selectedObject.GetComponents<Component>();
                foreach (var component in cameraSelector.selectedObject.GetComponents<Component>())
                {
                    string compName = component.GetType().Name;

                    if (advancedInspector)
                    {
                        CreateInspectorItem(compName, InspectorItemType.RemoveButton).AddListener(() =>
                        {
                            PlayAudio(removeComponent);
                            Destroy(component);
                        });
                    }
                    else
                    {
                        if (EditorComponentsList.GetMonoBehaviourTypes(true).Contains(component.GetType()))
                            if (component is PrefabObject || component is CubeObject || (component is Light && cameraSelector.selectedObject.GetComponent<PrefabObject>() != null))
                                CreateInspectorItem(compName, InspectorItemType.None);
                            else
                                CreateInspectorItem(compName, InspectorItemType.RemoveButton, infoButton : true, infoButtonDescription: EditorComponentsList.GetDescription(component)).AddListener(() =>
                                {
                                    Destroy(component);
                                    if (cameraSelector.selectedObject.GetComponent<CubeObject>() != null)
                                    {
                                        cameraSelector.selectedObject.GetComponent<Collider>()?.isTrigger = false;
                                        cameraSelector.selectedObject.GetComponent<NavMeshModifier>()?.ignoreFromBuild = false;
                                    }
                                    else if (cameraSelector.selectedObject.GetComponent<CubeObject>() == null)
                                    {
                                        CubeObject.Create(cameraSelector.selectedObject, MaterialChoser.materialTypes.Default);
                                        if (cameraSelector.selectedObject.GetComponent<Collider>() == null) cameraSelector.selectedObject.AddComponent<BoxCollider>();
                                        cameraSelector.selectedObject.GetComponent<Collider>()?.isTrigger = false;
                                        cameraSelector.selectedObject.GetComponent<NavMeshModifier>()?.ignoreFromBuild = false;
                                    }
                                    PlayAudio(removeComponent);
                                    Billboard.UpdateBillboards();
                                });
                    }

                        var type = component.GetType();
                    var fields = type.GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var field in fields)
                    {
                        if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                        {
                            if (!advancedInspector)
                            {
                                if (!EditorVariablesList.editorVariables.Any(iv => iv.varName == field.Name && iv.parentComponent == type) && field.Name != "enabled")
                                    continue;
                            }

                            var value = field.GetValue(component);
                            string valueStr = value != null ? value.ToString() : "null";

                            CreateInspectorVariable(EditorVariablesList.GetVariableDisplay(field.Name, type), value, field.FieldType, field, component, cameraSelector.selectedObject);
                        }
                    }

                    foreach (var prop in props)
                    {
                        if (prop.CanRead && prop.CanWrite && prop.GetMethod.GetParameters().Length == 0)
                        {
                            if (!advancedInspector)
                            {
                                if (!EditorVariablesList.editorVariables.Any(iv => iv.varName == prop.Name && iv.parentComponent == type))
                                    continue;
                            }

                            var value = prop.GetValue(component);
                            string valueStr = value != null ? value.ToString() : "null";

                            CreateInspectorVariable(EditorVariablesList.GetVariableDisplay(prop.Name, type), value, prop.PropertyType, prop, component, cameraSelector.selectedObject);
                        }
                    }
                }
            }
        }

        public static object GetMemberValue(object member, object target)
        {
            if (member is FieldInfo field)
                return field.GetValue(target);
            else if (member is PropertyInfo prop)
                return prop.GetValue(target);
            else
                throw new ArgumentException("Member must be a FieldInfo or PropertyInfo", nameof(member));
        }

        void SetMemberValue(object member, object target, object value)
        {
            switch (member)
            {
                case FieldInfo f:
                    f.SetValue(target, value);
                    break;

                case PropertyInfo p:
                    if (p.CanWrite)
                        p.SetValue(target, value);
                    break;

                default:
                    throw new ArgumentException("Unsupported member type: " + member.GetType());
            }
        }

        Type[] supportedTypes = new Type[]
            {
                typeof(string),
                typeof(int),
                typeof(float),
                typeof(double),
                typeof(bool),
                typeof(Vector3),
                typeof(Vector2),
                typeof(Vector4),
                typeof(Color),
            };

        Type choosing_type = null;
        object choosing_field = null;
        Component choosing_comp = null;
        int choosing_index = 0;
        bool choosing = false;
        string choosing_special = "";
        GameObject choosing_object = null;

        void CreateInspectorVariable(string fieldName, object value, Type type, object field, Component comp, GameObject obj)
        {
            string valueStr = value != null ? value.ToString() : "null";
            if (type == typeof(Color))
                valueStr = value != null ? (new Vector3(((Color)value).r * 255f, ((Color)value).g * 255f, ((Color)value).b * 255f)).ToString() : "null";
            if (type == typeof(bool))
            {
                CreateInspectorItem(fieldName, InspectorItemType.Button, valueStr, value).AddListener(() =>
                {
                    bool newValue = !(bool)value;
                    SetMemberValue(field, comp, newValue);
                    UpdateInspector();
                });
            }
            else if (supportedTypes.Contains(type)) CreateInspectorItem(fieldName, InspectorItemType.InputField, valueStr, value).AddListener(() =>
            {
                if (logShit)
                    Plugin.LogInfo(lastFieldText);

                if (type == typeof(string))
                    SetMemberValue(field, comp, lastFieldText);

                else if (type == typeof(int))
                    SetMemberValue(field, comp, int.Parse(lastFieldText));

                else if (type == typeof(float))
                    SetMemberValue(field, comp, float.Parse(lastFieldText));

                else if (type == typeof(double))
                    SetMemberValue(field, comp, double.Parse(lastFieldText));

                else if (type == typeof(bool))
                    SetMemberValue(field, comp, lastFieldText.ToLower() == "true");

                else if (type == typeof(Vector3))
                    SetMemberValue(field, comp, ParseHelper.ParseVector3(lastFieldText));

                else if (type == typeof(Vector2))
                    SetMemberValue(field, comp, ParseHelper.ParseVector2(lastFieldText));

                else if (type == typeof(Vector4))
                    SetMemberValue(field, comp, ParseHelper.ParseVector4(lastFieldText));

                else if (type == typeof(Color)) {
                    Vector3 c = ParseHelper.ParseVector3(lastFieldText);
                    SetMemberValue(field, comp, new Color(c.x / 255f, c.y / 255f, c.z / 255f));
                }

                else
                    Plugin.LogInfo($"Unsupported field type: {type}");

                UpdateInspector();
            });
            else if (type != null && type.IsEnum)
            {
                if (value == null)
                    value = Enum.GetValues(type).GetValue(0);

                if (value != null && type != null)
                {
                    CreateInspectorItem(fieldName, InspectorItemType.Dropdown, value != null ? value.ToString() : "null", value).AddListener(() =>
                    {
                        if (logShit)
                            Plugin.LogInfo($"Setting field {fieldName} to enum value: {lastEnum}");

                        if (logShit)
                            Plugin.LogInfo($"Component type: {comp.GetType()}, Enum type: {lastEnumType}");
                        if (logShit)
                            Plugin.LogInfo($"Enum value: {lastEnum} ({lastEnum.GetType()})");

                        SetMemberValue(field, comp, Enum.Parse(lastEnumType, lastEnum.ToString()));

                        if (logShit)
                            Plugin.LogInfo($"After assignment: {((FieldInfo)field).GetValue(comp)}");

                        UpdateInspector();
                    });
                }
            }
            else if (type.IsArray && type != null)
            {
                if (value == null)
                    value = Array.CreateInstance(type.GetElementType(), 0);

                for (int i = 0; i < ((Array)value).Length; i++)
                {
                    int index = i;
                    var element = ((Array)value).GetValue(i);
                    arrayType = value.GetType().GetElementType();

                    string displayName;
                    if (element is UnityEngine.Object uobj && uobj)
                        displayName = uobj.name;
                    else
                        displayName = element?.ToString() ?? "null";

                    CreateInspectorItem("<size=10>" + fieldName + $"[{i}] </size>{displayName}", InspectorItemType.ArrayItem, ((Array)value).GetValue(i)?.ToString() ?? "null", ((Array)value).GetValue(i)).AddListener(() =>
                    {
                        if (lastFieldText == "change")
                        {
                            string selectable = "GameObject";

                            if (arrayType == typeof(GameObject))
                            {
                                if (Input.GetKey(Plugin.altKey) && element != null && element is GameObject go && go)
                                {
                                    cameraSelector.SelectObject(go);
                                    return;
                                }

                                choosing_comp = comp;
                                choosing_field = field;
                                choosing_special = "";
                                choosing_type = type;
                                choosing_index = index;
                                choosing = true;
                                choosing_object = obj;
                                if (choosing_comp.GetType() == typeof(ActivateArena))
                                {
                                    selectable = "Enemy";
                                    choosing_special = "enemy";
                                }
                                cameraSelector.selectionMode = CameraSelector.SelectionMode.Cursor;
                                SetMessageText($"Select a {selectable} and press ALT + S");
                            }
                        }
                        else if (lastFieldText == "remove")
                        {
                            if (choosing_field == field && choosing_comp == comp)
                            {
                                SetMessageText("");
                                choosing = false;
                            }

                            if (logShit)
                                Plugin.LogInfo($"value: {(value == null ? "null" : value.GetType().FullName)}");

                            var arr = value as Array;
                            if (logShit)
                                for (int j = 0; j < arr.Length; j++)
                                    Plugin.LogInfo($"arr[{j}] = {(arr.GetValue(j) == null ? "null" : arr.GetValue(j).ToString())}");

                            if (arr == null) return;
                            int len = arr.Length;
                            if (i < 0 || index >= len) return;

                            var elemType = type.GetElementType();
                            var newArr = Array.CreateInstance(elemType, len - 1);
                            int idx = 0;

                            for (int j = 0; j < len; j++)
                            {
                                if (j == index) continue;
                                newArr.SetValue(arr.GetValue(j), idx++);
                            }

                            SetMemberValue(field, comp, newArr);
                            UpdateInspector();
                        }
                    });
                }

                CreateInspectorItem(fieldName, InspectorItemType.Button, "Add").AddListener(() =>
                {
                    var listType = typeof(List<>).MakeGenericType(type.GetElementType());
                    var list = Activator.CreateInstance(listType) as System.Collections.IList;
                    var arr = (Array)value;
                    for (int j = 0; j < arr.Length; j++)
                    {
                        list.Add(arr.GetValue(j));
                    }
                    object newElement = null;
                    if (type.GetElementType() == typeof(string))
                        newElement = "";
                    else if (type.GetElementType().IsValueType)
                        newElement = Activator.CreateInstance(type.GetElementType());
                    else
                        newElement = null;
                    list.Add(newElement);
                    var newArr = Array.CreateInstance(type.GetElementType(), list.Count);
                    list.CopyTo(newArr, 0);
                    SetMemberValue(field, comp, newArr);
                    UpdateInspector();
                });
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (value == null)
                    value = Activator.CreateInstance(type);

                var list = (IList)value;
                var elemType = type.GetGenericArguments()[0];
                arrayType = elemType;

                for (int i = 0; i < list.Count; i++)
                {
                    int index = i;
                    var element = list[i];

                    string displayName;
                    if (element is UnityEngine.Object uobj && uobj)
                        displayName = uobj.name;
                    else
                        displayName = element?.ToString() ?? "null";

                    CreateInspectorItem("<size=10>" + fieldName + $"[{i}] </size>{displayName}", InspectorItemType.ArrayItem, element?.ToString() ?? "null", element).AddListener(() =>
                    {
                        if (lastFieldText == "change")
                        {
                            string selectable = "GameObject";

                            if (arrayType == typeof(GameObject))
                            {
                                if (Input.GetKey(Plugin.altKey) && element != null && element is GameObject go && go)
                                {
                                    cameraSelector.SelectObject(go);
                                    return;
                                }

                                choosing_comp = comp;
                                choosing_field = field;
                                choosing_special = "";
                                choosing_type = type;
                                choosing_index = index;
                                choosing = true;
                                choosing_object = obj;
                                if (choosing_comp.GetType() == typeof(ActivateArena))
                                {
                                    selectable = "Enemy";
                                    choosing_special = "enemy";
                                }
                                cameraSelector.selectionMode = CameraSelector.SelectionMode.Cursor;
                                SetMessageText($"Select a {selectable} and press ALT + S");
                            }
                        }
                        else if (lastFieldText == "remove")
                        {
                            if (logShit)
                                Plugin.LogInfo($"value: {(value == null ? "null" : value.GetType().FullName)}");

                            if (logShit)
                                for (int j = 0; j < list.Count; j++)
                                    Plugin.LogInfo($"list[{j}] = {(list[j] == null ? "null" : list[j].ToString())}");

                            if (list == null) return;
                            if (index < 0 || index >= list.Count) return;

                            list.RemoveAt(index);

                            SetMemberValue(field, comp, list);
                            UpdateInspector();
                        }
                    });
                }
            }
            else
            {
                CreateInspectorItem($"<color=red>{fieldName}", InspectorItemType.None, "", null);
            }
        }

        public void SelectObject(GameObject obj)
        {
            if (choosing && choosing_comp != null && choosing_field != null && choosing_type != null && choosing_object != null)
            {
                if (choosing_special == "enemy")
                {
                    GameObject originalObj = obj;
                    while (obj != null && obj.GetComponent<EnemyIdentifier>() == null)
                    {
                        if (obj.transform.parent == null)
                        {
                            obj = originalObj;
                            break;
                        }
                        obj = obj.transform.parent.gameObject;
                    }

                    obj.gameObject.SetActive(false);
                }

                if (obj != null)
                {
                    var arr = GetMemberValue(choosing_field, choosing_comp) as Array;
                    if (arr == null)
                    {
                        Plugin.LogError("Array is null, can't assign.");
                        return;
                    }

                    if (choosing_index < 0 || choosing_index >= arr.Length)
                    {
                        Plugin.LogError($"Invalid index {choosing_index} for array of length {arr.Length}.");
                        return;
                    }

                    var elemType = choosing_type.GetElementType();
                    var newArr = Array.CreateInstance(elemType, arr.Length);

                    for (int i = 0; i < arr.Length; i++)
                    {
                        newArr.SetValue(i == choosing_index ? obj : arr.GetValue(i), i);
                    }

                    SetMemberValue(choosing_field, choosing_comp, newArr);
                    UpdateInspector();

                    cameraSelector.SelectObject(choosing_object);

                    SetMessageText("");

                    choosing = false;

                    if (logShit)
                        Plugin.LogInfo($"Replaced element {choosing_index} with {cameraSelector.selectedObject.name}");
                }
            }
        }

        void SetMessageText(string txt)
        {
            editorCanvas.transform.GetChild(0).GetChild(7).GetComponent<TMP_Text>().text = txt;
        }

        UnityEvent CreateInspectorItem(string DisplayName, InspectorItemType itemType, string defaultValue = "", object value = null, int forceChildIndex = -1, bool infoButton = false, string infoButtonDescription = "Null.")
        {
            GameObject content = editorCanvas.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(3).gameObject;
            GameObject templateItem = content.transform.GetChild(0).gameObject;

            GameObject newItem = Instantiate(templateItem, content.transform);
            newItem.name = $"Item_{DisplayName}";
            newItem.SetActive(true);
            newItem.GetComponentInChildren<TMP_Text>().text = DisplayName;

            if (forceChildIndex > 0)
            {
                newItem.transform.SetSiblingIndex(forceChildIndex);
            }

            GameObject field = newItem.transform.GetChild(1).gameObject;
            GameObject removeButton = newItem.transform.GetChild(2).gameObject;
            GameObject button = newItem.transform.GetChild(3).gameObject;
            GameObject copyButton = newItem.transform.GetChild(4).gameObject;
            GameObject pasteButton = newItem.transform.GetChild(5).gameObject;
            GameObject dropdown = newItem.transform.GetChild(6).gameObject;
            GameObject descriptionButton = newItem.transform.GetChild(7).gameObject;

            field.SetActive(false);
            removeButton.SetActive(false);
            button.SetActive(false);
            copyButton.SetActive(false);
            pasteButton.SetActive(false);
            dropdown.SetActive(false);
            descriptionButton.SetActive(infoButton);

            if (infoButton)
            {
                descriptionButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    PopupManager.CreatePopup("Info popup", infoButtonDescription);
                });
            }

            if (itemType == InspectorItemType.InputField)
            {
                field.SetActive(true);

                field.GetComponentInChildren<TMP_InputField>().text = defaultValue;

                UnityEvent e = new UnityEvent();

                field.GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener((string text) =>
                {
                    lastFieldText = text;
                    e.Invoke();
                });

                //copyButton.SetActive(true);
                //pasteButton.SetActive(true);

                copyButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    coppiedValue = value;
                    lastCoppiedType = value != null ? value.GetType() : null;
                    if (logShit)
                        Plugin.LogInfo($"Copied value: {coppiedValue} of type: {lastCoppiedType}");
                });

                pasteButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if (coppiedValue != null && value != null && coppiedValue.GetType() == value.GetType())
                    {
                        if (value is string)
                            lastFieldText = (string)coppiedValue;
                        else
                            lastFieldText = coppiedValue.ToString();
                        field.GetComponentInChildren<TMP_InputField>().text = lastFieldText;
                        if (logShit)
                            Plugin.LogInfo($"Pasted value: {lastFieldText}");
                        e.Invoke();
                    }
                    else
                    {
                        if (logShit)
                            Plugin.LogInfo("No compatible value to paste.");
                    }
                });

                return e;
            }

            if (itemType == InspectorItemType.RemoveButton)
            {
                removeButton.SetActive(true);

                return removeButton.GetComponentInChildren<Button>().onClick;
            }

            if (itemType == InspectorItemType.Button)
            {
                button.SetActive(true);

                button.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text = defaultValue;

                return button.GetComponentInChildren<Button>().onClick;
            }

            if (itemType == InspectorItemType.ArrayItem)
            {
                copyButton.SetActive(true);
                pasteButton.SetActive(true);

                copyButton.GetComponentInChildren<TMP_Text>().text = "Change";
                pasteButton.GetComponentInChildren<TMP_Text>().text = "Remove";

                UnityEvent e = new UnityEvent();

                copyButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    lastFieldText = "change";
                    e.Invoke();
                });

                pasteButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    lastFieldText = "remove";
                    e.Invoke();
                });

                return e;
            }

            if (itemType == InspectorItemType.Dropdown)
            {
                dropdown.SetActive(true);

                dropdown.GetComponentInChildren<TMP_Dropdown>().options = new List<TMP_Dropdown.OptionData>();
                lastEnumType = value.GetType();
                UnityEvent e = new UnityEvent();

                int i = 0, selectIndex = 0;

                foreach (var ee in Enum.GetValues(lastEnumType))
                {
                    dropdown.GetComponentInChildren<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(ee.ToString()));

                    if (logShit)
                        Plugin.LogInfo(ee.ToString() + value.ToString());
                    if (ee.ToString() == value.ToString())
                        selectIndex = i;

                    i++;
                }

                dropdown.GetComponentInChildren<TMP_Dropdown>().value = selectIndex;

                dropdown.GetComponentInChildren<TMP_Dropdown>().onValueChanged.AddListener((int i) =>
                {
                    lastEnumType = value.GetType();
                    lastEnum = (Enum)Enum.GetValues(lastEnumType).GetValue(i);

                    e.Invoke();
                });

                return e;
            }

            return null;
        }

        void CreateHierarchyItem(GameObject obj, string backupName = "Null object", string backupDescription = "Null description", bool goToParent = false, Color? forceColorMultiplier = null)
        {
            GameObject content = editorCanvas.transform.GetChild(0).GetChild(2).GetChild(3).gameObject;
            GameObject templateItem = content.transform.GetChild(0).gameObject;

            GameObject newItem = Instantiate(templateItem, content.transform);
            newItem.name = obj != null ? ("Item_" + obj.name) : backupName;
            newItem.SetActive(true);
            newItem.transform.GetChild(0).GetComponent<TMP_Text>().text = obj != null ? $"{(obj.transform.childCount > 0 ? "> " : "")}{obj.name}" : backupName;
            newItem.transform.GetChild(1).GetComponent<TMP_Text>().text = backupDescription;
            Button button = newItem.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (goToParent)
            {
                ClickableWithKey c = newItem.AddComponent<ClickableWithKey>();
                c.button = button;
            }
            button.onClick.AddListener(() =>
            {
                if (goToParent)
                {
                    PlayAudio(unselectObject);
                    cameraSelector.selectedObject = cameraSelector.selectedObject.transform.parent != null ? cameraSelector.selectedObject.transform.parent.gameObject : null;
                    if (cameraSelector.selectedObject != null)
                    {
                        cameraSelector.SelectObject(cameraSelector.selectedObject);
                        cameraSelector.FocusOnSelected();
                    }
                }
                else
                {
                    if (Input.GetKey(Plugin.altKey))
                    {
                        SelectObject(obj);
                        holdingObject = null;
                        holdingTarget = null;
                    }
                    else
                    {
                        cameraSelector.SelectObject(obj);
                        cameraSelector.FocusOnSelected();
                    }
                }
            });

            newItem.gameObject.SetActive(true);

            if (obj != null)
            {
                if (cameraSelector.selectedObject == obj)
                    newItem.GetComponent<Image>().color = new Color(0, 0, newItem.GetComponent<Image>().color.b);

                if (obj.activeSelf)
                    newItem.GetComponent<Image>().color = newItem.GetComponent<Image>().color * new Color(1, 1, 1, 1);
                else
                    newItem.GetComponent<Image>().color = newItem.GetComponent<Image>().color * new Color(1, 1, 1, 0.5f);
            }

            if (forceColorMultiplier != null)
                newItem.GetComponent<Image>().color = newItem.GetComponent<Image>().color * forceColorMultiplier.Value;

            bool parent = false;
            if (obj == null && cameraSelector.selectedObject != null)
            {
                obj = (cameraSelector.selectedObject.transform.parent != null) ? cameraSelector.selectedObject.transform.parent.gameObject : null;
                parent = true;
            }

            EventTrigger eventTrigger = newItem.GetComponent<EventTrigger>();

            if (!parent)
            {
                eventTrigger.triggers.Clear();
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit,
                    callback = new EventTrigger.TriggerEvent()
                });
            }

            
            eventTrigger.triggers[0].callback.AddListener((data) =>
            {
                if (obj == null) return;
                holdingObject = obj;
                if (logShit)
                    Plugin.LogInfo($"Holding object: {holdingObject.name}");
            });
            
            eventTrigger.triggers[2].callback.AddListener((data) =>
            {
                if (obj == null) return;
                if (obj != null && logShit)
                    Plugin.LogInfo($"Entered object: {obj.name}");
                if (holdingObject != null && holdingObject != obj)
                {
                    if (obj == null)
                        holdingTarget = holdingObject;
                    else
                        holdingTarget = obj;
                }
            });
            
            eventTrigger.triggers[3].callback.AddListener((data) =>
            {
                if (obj == null) return;
                holdingTarget = null;
            });
        }

        public static List<string> GetAllScenes()
        {
            string appPath = Application.persistentDataPath;

            string path = appPath + "\\ULTRAEDITOR";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            List<string> fileNames = new List<string>();

            foreach (var item in Directory.GetFiles(path))
            {
                if (item.EndsWith(".uterus"))
                    fileNames.Add(item.Replace(path + "\\", ""));
            }

            return fileNames;
        }

        string lastLoaded = "";
        public void TryToSaveShit()
        {
            GameObject saveScenePopup = editorCanvas.transform.GetChild(0).GetChild(9).gameObject;
            TMP_InputField field = saveScenePopup.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>();
            TMP_Text foundComponents = saveScenePopup.transform.GetChild(2).GetComponent<TMP_Text>();
            Button addButton = saveScenePopup.transform.GetChild(4).GetComponent<Button>();

            saveScenePopup.SetActive(true);

            field.Select();

            field.onValueChanged.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();
            field.onValueChanged.AddListener((string val) =>
            {
                foundComponents.text = "Save scene:\n";
                foundComponents.text += $"{field.text}<color=grey>.uterus";
            });

            if (lastLoaded != "")
            {
                field.text = lastLoaded.Replace(".uterus", "");
                field.onValueChanged.Invoke(lastLoaded.Replace(".uterus", ""));
            }

            addButton.onClick.AddListener(() =>
            {
                saveScenePopup.SetActive(false);
                SaveShit(field.text);
            });
        }

        public void SaveShit(string path)
        {
            SetAlert("Couldn't save scene!", col : new Color(1, 0f, 0f));
            string text = GetSceneJson();

            File.WriteAllText(Application.persistentDataPath + $"/ULTRAEDITOR/{path}.uterus", text);
            SetAlert("Scene saved!", "Info!", col: new Color(0.25f, 1f, 0.25f));
        }

        public string GetSceneJson()
        {
            return SceneJsonSaver.GetSceneJson();
            return Saving.GetSceneOld();
        }

        public static void StaticLoadPopup(GameObject canvas)
        {
            GameObject loadScenePopup = canvas.transform.GetChild(0).GetChild(8).gameObject;
            TMP_InputField field = loadScenePopup.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>();
            TMP_Text foundComponents = loadScenePopup.transform.GetChild(2).GetComponent<TMP_Text>();
            Button addButton = loadScenePopup.transform.GetChild(4).GetComponent<Button>();

            loadScenePopup.SetActive(true);

            field.Select();

            field.onValueChanged.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();
            field.onValueChanged.AddListener((string val) =>
            {
                sceneResults.Clear();

                List<string> scenes = GetAllScenes();

                foreach (string type in scenes)
                {
                    float accuracy = 0f;
                    string typeName = type.ToLower();
                    string searchName = val.ToLower();
                    int minLength = Math.Min(typeName.Length, searchName.Length);
                    for (int i = 0; i < minLength; i++)
                    {
                        if (typeName[i] == searchName[i])
                            accuracy += 1f / minLength;
                        else
                            break;
                    }
                    if (typeName.Contains(searchName))
                        accuracy += 0.5f;

                    if (typeName == searchName)
                        accuracy += 10000f;

                    if (accuracy > 0f)
                        sceneResults.Add((type, accuracy));
                }

                sceneResults = sceneResults.OrderByDescending(t => t.Item2).ToList();

                foundComponents.text = "Found saves:\n";
                foreach (var result in sceneResults.Take(3))
                {
                    foundComponents.text += $"{result.Item1}<color=grey>   ";
                }
            });

            addButton.onClick.AddListener(() =>
            {
                if (sceneResults.Count > 0)
                {
                    string sceneName = sceneResults[0].Item1;
                    if (sceneName != null)
                    {
                        loadScenePopup.SetActive(false);
                        canOpenEditor = false;
                        EmptySceneLoader.forceSave = sceneName;
                        EmptySceneLoader.forceEditor = false;
                        EmptySceneLoader.forceLevelCanOpenEditor = false;
                        EmptySceneLoader.Instance.LoadLevel();
                    }
                }
            });
        }

        public void GroupName(string n)
        {
            List<GameObject> objects = [];

            foreach (var o in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList())
            {
                if (o.GetComponent<SavableObject>() != null)
                    if (o.name == n || (n.StartsWith("*") && o.name.Contains(n.Replace("*", ""))))
                        objects.Add(o);
            }

            GameObject group = createCube(pos: new Vector3(0.00f, 90.00f, 4.25f), layer: "Invisible", objName: $"Group of {n}", matType: MaterialChoser.materialTypes.NoCollision);
            foreach (var o in objects)
            {
                o.transform.SetParent(group.transform, true);
            }

            SetAlert("Group made!", "Info!", col: new Color(0.25f, 1f, 0.25f));
        }

        public void TryToGroupName()
        {
            GameObject loadScenePopup = editorCanvas.transform.GetChild(0).GetChild(18).gameObject;
            TMP_InputField field = loadScenePopup.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>();
            TMP_Text foundComponents = loadScenePopup.transform.GetChild(2).GetComponent<TMP_Text>();
            Button addButton = loadScenePopup.transform.GetChild(4).GetComponent<Button>();

            loadScenePopup.SetActive(true);

            field.Select();

            if (cameraSelector.selectedObject != null)
                field.text = cameraSelector.selectedObject.name;

            field.onValueChanged.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();
            field.onValueChanged.AddListener((string val) =>
            {

            });

            addButton.onClick.AddListener(() =>
            {
                loadScenePopup.SetActive(false);
                GroupName(field.text);
            });
        }

        public void TryToLoadShit()
        {
            GameObject loadScenePopup = editorCanvas.transform.GetChild(0).GetChild(8).gameObject;
            TMP_InputField field = loadScenePopup.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>();
            TMP_Text foundComponents = loadScenePopup.transform.GetChild(2).GetComponent<TMP_Text>();
            Button addButton = loadScenePopup.transform.GetChild(4).GetComponent<Button>();

            loadScenePopup.SetActive(true);

            field.Select();

            field.onValueChanged.RemoveAllListeners();
            addButton.onClick.RemoveAllListeners();
            field.onValueChanged.AddListener((string val) =>
            {
                sceneResults.Clear();

                List<string> scenes = GetAllScenes();

                foreach (string type in scenes)
                {
                    float accuracy = 0f;
                    string typeName = type.ToLower();
                    string searchName = val.ToLower();
                    int minLength = Math.Min(typeName.Length, searchName.Length);
                    for (int i = 0; i < minLength; i++)
                    {
                        if (typeName[i] == searchName[i])
                            accuracy += 1f / minLength;
                        else
                            break;
                    }
                    if (typeName.Contains(searchName))
                        accuracy += 0.5f;

                    if (typeName == searchName)
                        accuracy += 10000f;

                    if (accuracy > 0f)
                        sceneResults.Add((type, accuracy));
                }

                sceneResults = [.. sceneResults.OrderByDescending(t => t.Item2)];

                foundComponents.text = "Found saves:\n";
                foreach (var result in sceneResults.Take(3))
                {
                    foundComponents.text += $"{result.Item1}<color=grey>   ";
                }
            });

            addButton.onClick.AddListener(() =>
            {
                loadScenePopup.SetActive(false);
                if (sceneResults.Count > 0)
                {
                    string sceneName = sceneResults[0].Item1;
                    if (sceneName != null)
                    {
                        StartCoroutine(LoadScene(sceneName));
                    }
                }
            });
        }

        public IEnumerator LoadScene(string sceneName)
        {
            DeleteScene(true);
            yield return new WaitForEndOfFrame();
            LoadShit(sceneName);
            yield return new WaitForEndOfFrame();
            SetAlert("Scene loaded!", "Info!", new Color(1, 0.5f, 0.25f));
            Billboard.UpdateBillboards();
        }

        public void LoadShit(string sceneName)
        {
            string path = Application.persistentDataPath + $"/ULTRAEDITOR/{sceneName}";

            if (!File.Exists(path))
            {
                Plugin.LogError("Save not found!");
                return;
            }

            LoadingHelper.cachedIds.Clear();

            string text = File.ReadAllText(path);

            Plugin.LogInfo($"Loading {sceneName}...");
            LoadSceneFile(text);

            if (MissionNameText != null)
            {
                Destroy(MissionNameText.GetComponent<LevelNameFinder>());
                MissionNameText.text = sceneName.Replace(".uterus", "");
                lastLoaded = sceneName;
            }

        }

        public void LoadSceneFile(string text)
        {
            Log("Trying to load scene file...");

            if (text.StartsWith("{"))
            {
                LoadingHelper.legacyIDs = false;
                SceneJsonSaver.LoadSceneJson(text);
                return;
            }

            LoadingHelper.legacyIDs = true;
            float startTime = Time.realtimeSinceStartup;

            int lineIndex = 0;
            int phase = 0;
            bool isInScript = false;
            string scriptType = "";
            List<string> passes = [];
            GameObject workingObject = null;

            foreach (var line in text.Split(["\r\n", "\n"], StringSplitOptions.None))
            {
                if (!isInScript && line.StartsWith("? ") && line.EndsWith(" ?"))
                {
                    scriptType = line.Replace(" ?", "").Replace("? ", "");
                    isInScript = true;
                    lineIndex = 0;
                    phase = 0;

                    workingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    workingObject.AddComponent<SpawnedObject>();
                }
                else if (isInScript)
                {
                    if (logShit)
                        Plugin.LogInfo($"Line {lineIndex} \"{line}\" type \"{scriptType}\" phase {phase}");

                    if (line == "? END ?")
                    {
                        isInScript = false;
                        lineIndex++;
                        continue;
                    }
                    if (line == "? PASS ?")
                    {
                        phase++;
                        lineIndex++;
                        continue;
                    }
                    if (line == "")
                    {
                        lineIndex++;
                        continue;
                    }

                    if (lineIndex == 1)
                        workingObject.GetComponent<SpawnedObject>().ID = line;
                    if (lineIndex == 2)
                        workingObject.name = line;
                    if (lineIndex == 3)
                        workingObject.layer = int.Parse(line);
                    if (lineIndex == 4)
                        workingObject.tag = line;
                    if (lineIndex == 5)
                        workingObject.transform.position = ParseHelper.ParseVector3(line);
                    if (lineIndex == 6)
                        workingObject.transform.eulerAngles = ParseHelper.ParseVector3(line);
                    if (lineIndex == 7)
                        workingObject.transform.localScale = ParseHelper.ParseVector3(line);
                    if (lineIndex == 8)
                        workingObject.gameObject.SetActive(line == "1");
                    if (lineIndex == 9)
                        workingObject.GetComponent<SpawnedObject>().parentID = line;

                    if (lineIndex >= 10)
                    {
                        passes.Append(line);
                    }
                    if (lineIndex == 10 && scriptType == "CubeObject")
                        CubeObject.Create(workingObject, (MaterialChoser.materialTypes)Enum.GetValues(typeof(MaterialChoser.materialTypes)).GetValue(int.Parse(line)));
                    if (lineIndex >= 10 && scriptType == "CubeObject")
                    {
                        if (phase == 1)
                            workingObject.GetComponent<CubeObject>().matTiling = float.Parse(line);
                        else if (phase == 2)
                            workingObject.GetComponent<CubeObject>().isTrigger = line.ToLower() == "true";
                        else if (phase == 3)
                            workingObject.GetComponent<CubeObject>().shape = (MaterialChoser.shapes)Enum.GetValues(typeof(MaterialChoser.shapes)).GetValue(int.Parse(line));
                    }

                    if (lineIndex == 10 && scriptType == "PrefabObject")
                    {
                        GameObject newObj = SpawnAsset(line, true);
                        newObj.transform.position = workingObject.transform.position;
                        newObj.transform.eulerAngles = workingObject.transform.eulerAngles;
                        newObj.transform.localScale = workingObject.transform.localScale;
                        newObj.layer = workingObject.layer;
                        newObj.tag = workingObject.tag;
                        newObj.name = workingObject.name;
                        newObj.SetActive(workingObject.activeSelf);
                        newObj.AddComponent<SpawnedObject>();
                        newObj.GetComponent<SpawnedObject>().ID = workingObject.GetComponent<SpawnedObject>().ID;
                        newObj.GetComponent<SpawnedObject>().parentID = workingObject.GetComponent<SpawnedObject>().parentID;
                        Destroy(workingObject);

                        if (newObj.GetComponent<Door>() != null)
                        {
                            var m = newObj.GetComponent<Door>().GetType().GetMethod("GetPos",
                                BindingFlags.Instance | BindingFlags.NonPublic);

                            m.Invoke(newObj.GetComponent<Door>(), null);

                        }
                    }

                    if (lineIndex == 10 && scriptType == "ArenaObject")
                    {
                        ArenaObject.Create(workingObject);
                        workingObject.GetComponent<ArenaObject>().onlyWave = line.ToLower() == "true";
                    }
                    if (lineIndex >= 11 && scriptType == "ArenaObject")
                        workingObject.GetComponent<ArenaObject>().addId(line);

                    if (lineIndex == 10 && scriptType == "NextArenaObject")
                    {
                        NextArenaObject.Create(workingObject);
                        workingObject.GetComponent<NextArenaObject>().lastWave = line.ToLower() == "true";
                    }
                    if (lineIndex == 11 && scriptType == "NextArenaObject")
                        workingObject.GetComponent<NextArenaObject>().enemyCount = int.Parse(line);
                    if (lineIndex >= 12 && scriptType == "NextArenaObject")
                    {
                        if (phase == 0)
                            workingObject.GetComponent<NextArenaObject>().addEnemyId(line);
                        else if (phase == 1)
                            workingObject.GetComponent<NextArenaObject>().addToActivateId(line);
                    }

                    if (scriptType == "ActivateObject" && workingObject.GetComponent<ActivateObject>() == null)
                        ActivateObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "ActivateObject")
                        if (phase == 0)
                            workingObject.GetComponent<ActivateObject>().addToActivateId(line);
                        else if (phase == 1)
                            workingObject.GetComponent<ActivateObject>().addtoDeactivateId(line);
                        else if (phase == 2)
                            workingObject.GetComponent<ActivateObject>().canBeReactivated = line.ToLower() == "true";
                        else if (phase == 3)
                            workingObject.GetComponent<ActivateObject>().delay = float.Parse(line);

                    if (scriptType == "CheckpointObject" && workingObject.GetComponent<CheckpointObject>() == null)
                        CheckpointObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "CheckpointObject")
                        if (phase == 0)
                            workingObject.GetComponent<CheckpointObject>().addRoomId(line);
                        else if (phase == 1)
                            workingObject.GetComponent<CheckpointObject>().addRoomToInheritId(line);

                    if (scriptType == "DeathZone" && workingObject.GetComponent<DeathZoneObject>() == null)
                        DeathZoneObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "DeathZone")
                        if (phase == 0)
                            workingObject.GetComponent<DeathZoneObject>().notInstaKill = line.ToLower() == "true";
                        else if (phase == 1)
                            workingObject.GetComponent<DeathZoneObject>().damage = int.Parse(line);
                        else if (phase == 2)
                            workingObject.GetComponent<DeathZoneObject>().affected = (AffectedSubjects)Enum.GetValues(typeof(AffectedSubjects)).GetValue(int.Parse(line));

                    if (scriptType == "Light" && workingObject.GetComponent<LightObject>() == null)
                        LightObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "Light")
                        if (phase == 0)
                            workingObject.GetComponent<LightObject>().intensity = float.Parse(line);
                        else if (phase == 1)
                            workingObject.GetComponent<LightObject>().range = float.Parse(line);
                        else if (phase == 2)
                            workingObject.GetComponent<LightObject>().type = (LightType)Enum.GetValues(typeof(LightType)).GetValue(int.Parse(line));
                        else if (phase == 3)
                            workingObject.GetComponent<LightObject>().color = ParseHelper.ParseVector3(line);

                    if (scriptType == "MusicObject" && workingObject.GetComponent<MusicObject>() == null)
                        MusicObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "MusicObject")
                        if (phase == 0)
                            workingObject.GetComponent<MusicObject>().calmThemePath = line;
                        else if (phase == 1)
                            workingObject.GetComponent<MusicObject>().battleThemePath = line;

                    if (scriptType == "SFXObject" && workingObject.GetComponent<SFXObject>() == null)
                        SFXObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "SFXObject")
                        if (phase == 0)
                            workingObject.GetComponent<SFXObject>().url = line;
                        else if (phase == 1)
                            workingObject.GetComponent<SFXObject>().disableAfterPlaying = line.ToLower() == "true";
                        else if (phase == 2)
                            workingObject.GetComponent<SFXObject>().playOnAwake = line.ToLower() == "true";
                        else if (phase == 3)
                            workingObject.GetComponent<SFXObject>().loop = line.ToLower() == "true";
                        else if (phase == 4)
                            workingObject.GetComponent<SFXObject>().range = float.Parse(line);
                        else if (phase == 5)
                            workingObject.GetComponent<SFXObject>().volume = float.Parse(line);

                    if (scriptType == "MovingPlatformAnimator" && workingObject.GetComponent<MovingPlatformAnimator>() == null)
                        MovingPlatformAnimator.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "MovingPlatformAnimator")
                        if (phase == 0)
                            workingObject.GetComponent<MovingPlatformAnimator>().addAffectedCubeId(line);
                        else if (phase == 1)
                            workingObject.GetComponent<MovingPlatformAnimator>().addPointId(line);
                        else if (phase == 2)
                            workingObject.GetComponent<MovingPlatformAnimator>().speed = float.Parse(line);
                        else if (phase == 3)
                            workingObject.GetComponent<MovingPlatformAnimator>().movesWithThePlayer = line.ToLower() == "true";
                        else if (phase == 4)
                            workingObject.GetComponent<MovingPlatformAnimator>().mode = (MovingPlatformAnimator.platformMode)Enum.GetValues(typeof(MovingPlatformAnimator.platformMode)).GetValue(int.Parse(line));

                    if (scriptType == "SkullActivatorObject" && workingObject.GetComponent<SkullActivatorObject>() == null)
                        SkullActivatorObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "SkullActivatorObject")
                        if (phase == 0)
                            workingObject.GetComponent<SkullActivatorObject>().acceptedItemType = (SkullActivatorObject.skullType)Enum.GetValues(typeof(SkullActivatorObject.skullType)).GetValue(int.Parse(line));
                        else if (phase == 1)
                            workingObject.GetComponent<SkullActivatorObject>().addToActivateId(line);
                        else if (phase == 2)
                            workingObject.GetComponent<SkullActivatorObject>().addToDeactivateId(line);
                        else if (phase == 3)
                            workingObject.GetComponent<SkullActivatorObject>().addTriggerAltarId(line);

                    if (scriptType == "CubeTilingAnimator" && workingObject.GetComponent<CubeTilingAnimator>() == null)
                        CubeTilingAnimator.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "CubeTilingAnimator")
                        if (phase == 0)
                            workingObject.GetComponent<CubeTilingAnimator>().addId(line);
                        else if (phase == 1)
                            workingObject.GetComponent<CubeTilingAnimator>().scrolling = ParseHelper.ParseVector2(line);

                    if (scriptType == "HUDMessageObject" && workingObject.GetComponent<HUDMessageObject>() == null)
                        HUDMessageObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "HUDMessageObject")
                        if (phase == 0)
                            workingObject.GetComponent<HUDMessageObject>().message = line;
                        else if (phase == 1)
                            workingObject.GetComponent<HUDMessageObject>().disableAfterShowing = line.ToLower() == "true";

                    if (scriptType == "TeleportObject" && workingObject.GetComponent<NewTeleportObject>() == null)
                        NewTeleportObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "TeleportObject")
                        if (phase == 0)
                            workingObject.GetComponent<NewTeleportObject>().teleportPosition = ParseHelper.ParseVector3(line);
                        else if (phase == 1)
                            workingObject.GetComponent<NewTeleportObject>().canBeReactivated = line.ToLower() == "true";
                        else if (phase == 2)
                            workingObject.GetComponent<NewTeleportObject>().slowdown = line.ToLower() == "true";

                    if (scriptType == "LevelInfoObject" && workingObject.GetComponent<LevelInfoObject>() == null)
                        LevelInfoObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "LevelInfoObject")
                        if (phase == 0)
                            workingObject.GetComponent<LevelInfoObject>().ambientColor = ParseHelper.ParseVector3(line);
                        else if (phase == 1)
                            workingObject.GetComponent<LevelInfoObject>().intensityMultiplier = float.Parse(line);
                        else if (phase == 2)
                            workingObject.GetComponent<LevelInfoObject>().changeLighting = line.ToLower() == "true";
                        else if (phase == 3)
                            workingObject.GetComponent<LevelInfoObject>().tipOfTheDay = line;
                        else if (phase == 4)
                            workingObject.GetComponent<LevelInfoObject>().levelLayer = line;
                        else if (phase == 5)
                            workingObject.GetComponent<LevelInfoObject>().playMusicOnDoorOpen = line.ToLower() == "true";
                        else if (phase == 6)
                            workingObject.GetComponent<LevelInfoObject>().levelName = line;
                }

                lineIndex++;
            }

            Plugin.LogInfo("Assigning parents...");

            var allObjs = GameObject.FindObjectsOfType<SpawnedObject>(true);

            var dict = new Dictionary<string, SpawnedObject>();
            foreach (var o in allObjs)
            {
                if (!string.IsNullOrEmpty(o.ID))
                    dict[o.ID] = o;
            }

            foreach (var obj in allObjs)
            {
                if (!string.IsNullOrEmpty(obj.parentID) && dict.TryGetValue(obj.parentID, out var parent))
                {
                    obj.transform.SetParent(parent.transform, true);
                }
            }

            Plugin.LogInfo("Creating objects...");

            foreach (var obj in allObjs)
            {
                obj.GetComponent<SavableObject>()?.Create();
            }

            Plugin.LogInfo($"Loading done in {Time.realtimeSinceStartup - startTime} seconds!");

            cameraSelector.selectedObject = null;
            cameraSelector.UnselectObject();
        }

        IEnumerator GoToBackupScene()
        {
            DeleteScene(true);
            yield return new WaitForEndOfFrame();
            LoadSceneFile(tempScene);
            yield return new WaitForEndOfFrame();
            SetAlert("Loaded scene backup", "Info!", new Color(1, 0.5f, 0.25f));
            Billboard.UpdateBillboards();
        }

        public void SetAlert(string str, string title = "Error!", Color? col = null)
        {
            GameObject alert = editorCanvas.transform.GetChild(0).GetChild(10).gameObject;
            alert.GetComponent<Animator>().speed = 1f;
            alert.SetActive(false);
            alert.SetActive(true);

            Color finalCol = col != null ? (Color)col : new Color(1, 0.25f, 0.25f);

            alert.transform.GetChild(0).GetComponent<TMP_Text>().text = title;
            alert.transform.GetChild(1).GetComponent<TMP_Text>().text = str;
            alert.transform.GetComponent<Image>().color = finalCol;
            PlayAudio(chord);
        }

        public void DisableAlert()
        {
            GameObject alert = editorCanvas.transform.GetChild(0).GetChild(10).gameObject;
            alert.SetActive(false);
        }

        public static void Log(string str)
        {
            Plugin.LogInfo($"[EditorManager] {str}");
        }

        public static AudioClip activateObject = BundlesManager.editorBundle.LoadAsset<AudioClip>("Speech On");
        public static AudioClip inactivateObject = BundlesManager.editorBundle.LoadAsset<AudioClip>("Speech Sleep");
        public static AudioClip selectObject = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Balloon");
        public static AudioClip unselectObject = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Default");
        public static AudioClip destroyObject = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Error");
        public static AudioClip spawnAsset = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Exclamation");
        public static AudioClip removeComponent = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Logoff Sound");
        public static AudioClip addComponent = BundlesManager.editorBundle.LoadAsset<AudioClip>("Windows Logon Sound");
        public static AudioClip chord = BundlesManager.editorBundle.LoadAsset<AudioClip>("chord");
        static AudioSource source = null;
        public static void PlayAudio(AudioClip clip)
        {
            if (source == null)
                source = new GameObject().AddComponent<AudioSource>();
            source.clip = clip;
            source.Play();
        }
    }
}