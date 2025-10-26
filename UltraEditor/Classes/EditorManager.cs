using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Component = UnityEngine.Component;

namespace UltraEditor.Classes
{
    public class EditorManager : MonoBehaviour
    {
        public static EditorManager Instance;

        public GameObject editorCanvas;
        public Camera editorCamera;
        public CameraSelector cameraSelector;
        public GameObject blocker;

        bool mouseLocked = true;
        bool advancedInspector = false;

        List<InspectorVariable> inspectorVariables = new List<InspectorVariable>();

        class InspectorVariable
        {
            public string varName;
            public Type parentComponent;
        }

        enum inspectorItemType
        {
            None,
            InputField,
            RemoveButton,
            Button,
            ArrayItem,
        }

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
            if (!mouseLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetKeyDown(Plugin.toggleEditorCanvasKey))
            {
                editorCanvas.SetActive(!editorCanvas.activeSelf);
                cameraSelector.ClearHover();
            }

            if (Input.GetKeyDown(Plugin.deleteObjectKey))
                deleteObject();

            if (Plugin.isToggleEnabledKeyPressed())
                toggleObject();

            if (Plugin.isDuplicateKeyPressed())
                duplicateObject();

            if (Input.GetKey(Plugin.createCubeKey))
            {
                createCube(true, false);
            }

            cameraSelector.enabled = (!blocker.activeSelf || cameraSelector.dragging) && editorCamera.gameObject.activeSelf;

            if (editorCanvas.activeSelf)
            {
                UpdateHierarchy();
            }

            if (Input.GetMouseButtonUp(0))
            {
                Plugin.LogInfo($"Released mouse button with " +
                    $"{(holdingObject ? holdingObject.name : "null")} & " +
                    $"{(holdingTarget ? holdingTarget.name : "null")}");

                if (holdingObject != null && holdingTarget != null && holdingObject != holdingTarget)
                {
                    Plugin.LogInfo($"Dropped object: {holdingObject.name} into target: {holdingTarget.name}");
                    holdingObject.transform.SetParent(holdingTarget.transform);
                    cameraSelector.selectedObject = holdingTarget;
                    lastSelected = null;
                    UpdateHierarchy();
                    holdingObject = null;
                    holdingTarget = null;
                }

                else if (holdingObject == holdingTarget && holdingObject != null)
                {
                    Plugin.LogInfo($"Released object: {holdingObject.name} from target");
                    holdingObject.transform.SetParent(null);
                    cameraSelector.selectedObject = holdingTarget;
                    lastSelected = null;
                    UpdateHierarchy();
                    holdingObject = null;
                    holdingTarget = null;
                }
            }

            if (Plugin.isSelectPressed() && cameraSelector.selectedObject != null)
            {
                SelectObject(cameraSelector.selectedObject);
            }
        }

        public void LateUpdate()
        {
            
        }

        static NavMeshSurface navMeshSurface;
        static string[] doNotDelete = new string[] { "GlobalLights", "Level Info", "FirstRoom", "StatsManager", "Canvas", "GameController", "Player", "EventSystem(Clone)", "CheatBinds", "PlatformerController(Clone)", "CheckPointsController" };
        public static void DeleteScene()
        {
            if (SceneHelper.CurrentScene == "Endless" && !StatsManager.Instance.timer)
            {
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (!doNotDelete.Contains(obj.name) && (Instance != null ? (obj != Instance.editorCamera.gameObject && obj != Instance.editorCanvas.gameObject && obj != Instance.gameObject) : true))
                    {
                        Destroy(obj);
                    }
                }

                if (navMeshSurface == null)
                {
                    GameObject navMeshObj = new GameObject("NavMeshSurface");
                    navMeshSurface = navMeshObj.AddComponent<NavMeshSurface>();
                    navMeshSurface.collectObjects = CollectObjects.All;
                    navMeshSurface.BuildNavMesh();
                    Plugin.LogInfo("NavMeshSurface created.");
                }
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
                mouseLocked = !mouseLocked;
                editorCamera.gameObject.SetActive(!mouseLocked);
                if (NewMovement.Instance != null)
                    editorCamera.transform.position = NewMovement.Instance.transform.position;
                editorCanvas.SetActive(!mouseLocked);
                if (NewMovement.Instance != null)
                    NewMovement.Instance.gameObject.SetActive(mouseLocked);
                blocker.SetActive(true);
                cameraSelector.ClearHover();
                cameraSelector.UnselectObject();
                cameraSelector.selectionMode = CameraSelector.SelectionMode.Cursor;

                if (mouseLocked)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }

                Time.timeScale = mouseLocked ? 1f : 0f;

                return;
            }

            GameObject prefab = BundlesManager.editorBundle.LoadAsset<GameObject>("EditorCanvas");

            if (prefab == null)
            {
                Plugin.LogError("Prefab 'EditorCanvas' not found in AssetBundle");
                return;
            }

            mouseLocked = false;

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
            SetupVariables();

            GameObject AssetsContainer = editorCanvas.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(4).gameObject;
            foreach (var item in AssetsContainer.transform.GetComponentsInChildren<Button>())
            {
                item.onClick.AddListener(() =>
                {
                    SpawnAsset(item.transform.GetChild(0).name);
                });
            }

            if (NewMovement.Instance != null)
            {
                NewMovement.Instance.gameObject.SetActive(mouseLocked);
                defaultCullingMask = CameraController.Instance.cam.cullingMask;
                ChangeCameraCullingLayers(defaultCullingMask);
            }
            Time.timeScale = mouseLocked ? 1f : 0f;

            SetupButtons();
        }

        void SetupButtons()
        {
            // File
            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                throw new NotImplementedException();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                throw new NotImplementedException();
            });

            // Edit
            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(1).GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                toggleObject();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(1).GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                deleteObject();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(1).GetChild(3).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                duplicateObject();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(1).GetChild(3).GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                if (navMeshSurface != null)
                {
                    navMeshSurface.BuildNavMesh();
                    Plugin.LogInfo("NavMesh rebuilt.");
                }
                else
                {
                    Plugin.LogError("No NavMeshSurface found to rebuild.");
                }
            });

            // Add
            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(2).GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(2).GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube(true);
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(2).GetChild(3).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                createCube(true, false);
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(2).GetChild(3).GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                createFloor();
            });

            // View
            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeCameraCullingLayers(-1);
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeCameraCullingLayers(defaultCullingMask);
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                advancedInspector = true;
                UpdateInspector();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                advancedInspector = false;
                UpdateInspector();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeLighting(0);
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(3).GetChild(3).GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeLighting(1);
            });
        }

        void SetupVariables()
        {
            inspectorVariables.Clear();

            NewInspectorVariable("localPosition", typeof(Transform));
            NewInspectorVariable("localEulerAngles", typeof(Transform));
            NewInspectorVariable("localScale", typeof(Transform));

            NewInspectorVariable("castShadows", typeof(MeshRenderer));
            NewInspectorVariable("receiveShadows", typeof(MeshRenderer));

            NewInspectorVariable("center", typeof(BoxCollider));
            NewInspectorVariable("size", typeof(BoxCollider));
            NewInspectorVariable("isTrigger", typeof(BoxCollider));

            NewInspectorVariable("center", typeof(CapsuleCollider));
            NewInspectorVariable("radius", typeof(CapsuleCollider));
            NewInspectorVariable("height", typeof(CapsuleCollider));
            NewInspectorVariable("isTrigger", typeof(CapsuleCollider));

            NewInspectorVariable("drag", typeof(Rigidbody));
            NewInspectorVariable("angularDrag", typeof(Rigidbody));
            NewInspectorVariable("mass", typeof(Rigidbody));
            NewInspectorVariable("useGravity", typeof(Rigidbody));
            NewInspectorVariable("isKinematic", typeof(Rigidbody));

            NewInspectorVariable("speed", typeof(NavMeshAgent));
            NewInspectorVariable("angularSpeed", typeof(NavMeshAgent));
            NewInspectorVariable("acceleration", typeof(NavMeshAgent));

            NewInspectorVariable("volume", typeof(AudioSource));
            NewInspectorVariable("pitch", typeof(AudioSource));
            NewInspectorVariable("loop", typeof(AudioSource));
            NewInspectorVariable("spatialize", typeof(AudioSource));
            NewInspectorVariable("spatialBlend", typeof(AudioSource));
            NewInspectorVariable("panStereo", typeof(AudioSource));
            NewInspectorVariable("reverbZoneMix", typeof(AudioSource));
            NewInspectorVariable("dopplerLevel", typeof(AudioSource));
            NewInspectorVariable("priority", typeof(AudioSource));
            NewInspectorVariable("minDistance", typeof(AudioSource));
            NewInspectorVariable("maxDistance", typeof(AudioSource));

            NewInspectorVariable("speed", typeof(Animator));

            // Enemies
            NewInspectorVariable("health", typeof(Zombie));
            NewInspectorVariable("health", typeof(Statue));
            NewInspectorVariable("originalHealth", typeof(Statue));

            // Triggers
            NewInspectorVariable("timed", typeof(HudMessage));
            NewInspectorVariable("message", typeof(HudMessage));
            NewInspectorVariable("timerTime", typeof(HudMessage));

            NewInspectorVariable("damage", typeof(DeathZone));
            NewInspectorVariable("notInstaKill", typeof(DeathZone));

            NewInspectorVariable("enemies", typeof(ActivateArena));
        }

        void NewInspectorVariable(string varName, Type parentComponent)
        {
            inspectorVariables.Add(new InspectorVariable
            {
                varName = varName,
                parentComponent = parentComponent
            });
        }

        void SpawnAsset(string dir)
        {
            GameObject obj = Instantiate(Plugin.Ass<GameObject>(dir));
            obj.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;

            if (Input.GetKey(Plugin.shiftKey) && cameraSelector.selectedObject != null)
            {
                obj.transform.SetParent(cameraSelector.selectedObject.transform);
                lastSelected = null;
            }
            else
                cameraSelector.SelectObject(obj);

            if (Input.GetKey(Plugin.altKey)) obj.SetActive(false);
        }

        void duplicateObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                cameraSelector.ClearSelectedMaterial();
                GameObject newObj = Instantiate(cameraSelector.selectedObject);
                cameraSelector.SelectObject(newObj);
            }
        }

        void deleteObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                Destroy(cameraSelector.selectedObject);
                cameraSelector.UnselectObject();
            }
        }

        void toggleObject()
        {
            if (cameraSelector.selectedObject != null)
            {
                lastHierarchy = new GameObject[0];
                cameraSelector.selectedObject.SetActive(!cameraSelector.selectedObject.activeSelf);
            }
        }

        void createCube(bool createRigidbody = false, bool useGravity = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;
            cube.transform.localScale = Vector3.one;
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
        }

        void createFloor()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;
            cube.transform.localScale = new Vector3(25,1,25);
            cube.layer = LayerMask.NameToLayer("Outdoors");
            cube.tag = "Floor";
            cameraSelector.SelectObject(cube);
        }

        void ChangeCameraCullingLayers(int layerMask)
        {
            editorCamera.cullingMask = layerMask;
        }

        void ChangeLighting(int lit)
        {
            if (lit == 1)
            {
                editorCamera.ResetReplacementShader();
            }
            else if (lit == 0)
            {
                Shader unlitShader = Shader.Find("Unlit/Texture");
                if (unlitShader != null)
                    editorCamera.SetReplacementShader(unlitShader, "");
                else
                    Plugin.LogError("Unlit shader not found");
            }
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

        Component[] lastComponents = new Component[0];
        GameObject[] lastHierarchy = new GameObject[0];
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
                    if (lastHierarchy[i] != rootObjects[i])
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
                CreateHierarchyItem(null, $"< {cameraSelector.selectedObject.name}", true, new Color(0.5f, 1, 0.5f, cameraSelector.selectedObject.activeSelf ? 1 : 0.5f));

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
                if (obj == editorCamera.gameObject || obj == this.gameObject || obj == editorCanvas.gameObject) continue;

                CreateHierarchyItem(obj);
            }

            UpdateInspector();
        }

        public static List<Type> GetAllMonoBehaviourTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            var engineAssemblyNames = new[]
            {
            "UnityEngine.CoreModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.UIModule",
            "UnityEngine.AnimationModule",
            "UnityEngine.AIModule",
            "UnityEngine.AudioModule",
            "UnityEngine.ParticleSystemModule"
        };

            foreach (var name in engineAssemblyNames)
            {
                var a = assemblies.FirstOrDefault(x => x.GetName().Name == name);
                if (a == null)
                {
                    try { assemblies.Add(Assembly.Load(name)); }
                    catch { }
                }
            }

            var types = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                })
                .Where(t => (t.IsSubclassOf(typeof(MonoBehaviour)) || typeof(Component).IsAssignableFrom(t)) && !t.IsAbstract)
                .ToList();

            return types;
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
                else if (typeof(System.Collections.IList).IsAssignableFrom(fieldType))
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

        string lastFieldText = "";
        object coppiedValue = null;
        Type lastCoppiedType = null;
        List<(string, Type, float)> searchResults = new List<(string, Type, float)>();
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

                CreateInspectorItem("Add component", inspectorItemType.Button, "Add").AddListener(() =>
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

                        var monoTypes = GetAllMonoBehaviourTypes();

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
                                if (c is ActivateArena)
                                {
                                    ActivateArena cc = (ActivateArena)c;
                                    cc.doors = new Door[0];
                                    cc.onlyWave = true;
                                }
                                if (c is HudMessage)
                                {
                                    HudMessage cc = (HudMessage)c;
                                    cc.timed = true;
                                }

                                UpdateInspector();
                            }
                            else
                            {
                                Plugin.LogError($"Component type '{componentName}' not found.");
                            }
                        }
                    });
                });

                CreateInspectorItem("Name", inspectorItemType.InputField, cameraSelector.selectedObject.name).AddListener(() =>
                {
                    cameraSelector.selectedObject.name = lastFieldText;
                    UpdateInspector();
                    lastSelected = null;
                });

                CreateInspectorItem("Tag", inspectorItemType.InputField, cameraSelector.selectedObject.tag).AddListener(() =>
                {
                    cameraSelector.selectedObject.tag = lastFieldText;
                    UpdateInspector();
                });

                CreateInspectorItem("Layer", inspectorItemType.InputField, LayerMask.LayerToName(cameraSelector.selectedObject.layer)).AddListener(() =>
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

                    CreateInspectorItem(compName, inspectorItemType.RemoveButton).AddListener(() =>
                    {
                        Destroy(component);
                    });

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
                                if (!inspectorVariables.Any(iv => iv.varName == field.Name && iv.parentComponent == type) && field.Name != "enabled")
                                    continue;
                            }

                            var value = field.GetValue(component);
                            string valueStr = value != null ? value.ToString() : "null";

                            CreateInspectorVariable(field.Name, value, field.FieldType, field, component, cameraSelector.selectedObject);
                        }
                    }

                    foreach (var prop in props)
                    {
                        if (prop.CanRead && prop.CanWrite && prop.GetMethod.GetParameters().Length == 0)
                        {
                            if (!advancedInspector)
                            {
                                if (!inspectorVariables.Any(iv => iv.varName == prop.Name && iv.parentComponent == type) && prop.Name != "enabled")
                                    continue;
                            }

                            var value = prop.GetValue(component);
                            string valueStr = value != null ? value.ToString() : "null";

                            CreateInspectorVariable(prop.Name, value, prop.PropertyType, prop, component, cameraSelector.selectedObject);
                        }
                    }
                }
            }
        }

        public static object GetMemberValue(object member, object target)
        {
            if (member is FieldInfo field)
            {
                return field.GetValue(target);
            }
            else if (member is PropertyInfo prop)
            {
                return prop.GetValue(target);
            }
            else
            {
                throw new ArgumentException("Member must be a FieldInfo or PropertyInfo", nameof(member));
            }
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
                    typeof(Vector4)
                };

        Type choosing_type = null;
        object choosing_field = null;
        Component choosing_comp = null;
        int choosing_index = 0;
        bool choosing = false;
        GameObject choosing_object = null;

        void CreateInspectorVariable(string fieldName, object value, Type type, object field, Component comp, GameObject obj)
        {
            string valueStr = value != null ? value.ToString() : "null";
            if (type == typeof(bool))
            {
                CreateInspectorItem(fieldName, inspectorItemType.Button, valueStr, value).AddListener(() =>
                {
                    bool newValue = !(bool)value;
                    SetMemberValue(field, comp, newValue);
                    UpdateInspector();
                });
            }
            else if (supportedTypes.Contains(type)) CreateInspectorItem(fieldName, inspectorItemType.InputField, valueStr, value).AddListener(() =>
            {
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
                    SetMemberValue(field, comp, ParseVector3(lastFieldText));

                else if (type == typeof(Vector2))
                    SetMemberValue(field, comp, ParseVector2(lastFieldText));

                else if (type == typeof(Vector4))
                    SetMemberValue(field, comp, ParseVector4(lastFieldText));

                else
                    Plugin.LogInfo($"Unsupported field type: {type}");

                UpdateInspector();
            });
            else if (type.IsArray)
            {
                if (value == null)
                    value = Array.CreateInstance(type.GetElementType(), 0);

                for (int i = 0; i < ((Array)value).Length; i++)
                {
                    int index = i;
                    CreateInspectorItem(fieldName + $"[{i}]", inspectorItemType.ArrayItem, ((Array)value).GetValue(i)?.ToString() ?? "null", ((Array)value).GetValue(i)).AddListener(() =>
                    {
                        if (lastFieldText == "change")
                        {
                            arrayType = value.GetType().GetElementType();

                            if (arrayType == typeof(GameObject))
                            {
                                choosing_comp = comp;
                                choosing_field = field;
                                choosing_type = type;
                                choosing_index = index;
                                choosing = true;
                                choosing_object = obj;
                                SetMessageText("Select a GameObject and press ALT + S");
                            }
                        }
                        else if (lastFieldText == "remove")
                        {
                            Plugin.LogInfo($"value: {(value == null ? "null" : value.GetType().FullName)}");

                            var arr = value as Array;
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

                CreateInspectorItem(fieldName, inspectorItemType.Button, "Add").AddListener(() =>
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
            else
            {
                CreateInspectorItem($"<color=red>{fieldName}", inspectorItemType.None, "", null);
            }
        }

        void SelectObject(GameObject obj)
        {
            if (choosing && choosing_comp != null && choosing_field != null && choosing_type != null && choosing_object != null)
            {
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

                    Plugin.LogInfo($"Replaced element {choosing_index} with {cameraSelector.selectedObject.name}");
                }
            }
        }

        void SetMessageText(string txt)
        {
            editorCanvas.transform.GetChild(0).GetChild(7).GetComponent<TMP_Text>().text = txt;
        }

        Vector4 ParseVector4(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 4)
                throw new FormatException($"Invalid Vector4 format: {input}");

            return new Vector4(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        Vector3 ParseVector3(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 3)
                throw new FormatException($"Invalid Vector3 format: {input}");

            return new Vector3(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        Vector2 ParseVector2(string input)
        {
            input = input.Trim('(', ')', ' ');
            var parts = input.Split(',');

            if (parts.Length != 2)
                throw new FormatException($"Invalid Vector2 format: {input}");

            return new Vector2(
                float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        UnityEvent CreateInspectorItem(string DisplayName, inspectorItemType itemType, string defaultValue = "", object value = null, int forceChildIndex = -1)
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

            field.SetActive(false);
            removeButton.SetActive(false);
            button.SetActive(false);
            copyButton.SetActive(false);
            pasteButton.SetActive(false);

            if (itemType == inspectorItemType.InputField)
            {
                field.SetActive(true);

                field.GetComponentInChildren<TMP_InputField>().text = defaultValue;

                UnityEvent e = new UnityEvent();

                field.GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener((string text) =>
                {
                    lastFieldText = text;
                    e.Invoke();
                });

                copyButton.SetActive(true);
                pasteButton.SetActive(true);

                copyButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    coppiedValue = value;
                    lastCoppiedType = value != null ? value.GetType() : null;
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
                        Plugin.LogInfo($"Pasted value: {lastFieldText}");
                        e.Invoke();
                    }
                    else
                    {
                        Plugin.LogInfo("No compatible value to paste.");
                    }
                });

                return e;
            }

            if (itemType == inspectorItemType.RemoveButton)
            {
                removeButton.SetActive(true);

                return removeButton.GetComponentInChildren<Button>().onClick;
            }

            if (itemType == inspectorItemType.Button)
            {
                button.SetActive(true);

                button.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text = defaultValue;

                return button.GetComponentInChildren<Button>().onClick;
            }

            if (itemType == inspectorItemType.ArrayItem)
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

            return null;
        }

        void CreateHierarchyItem(GameObject obj, string backupName = "Null object", bool goToParent = false, Color? forceColorMultiplier = null)
        {
            GameObject content = editorCanvas.transform.GetChild(0).GetChild(2).GetChild(3).gameObject;
            GameObject templateItem = content.transform.GetChild(0).gameObject;

            GameObject newItem = Instantiate(templateItem, content.transform);
            newItem.name = obj != null ? ("Item_" + obj.name) : backupName;
            newItem.SetActive(true);
            newItem.GetComponentInChildren<TMP_Text>().text = obj != null ? $"{(obj.transform.childCount > 0 ? "> " : "")}{obj.name}" : backupName;
            Button button = newItem.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (goToParent)
                {
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

            if (obj == null && cameraSelector.selectedObject != null)
            {
                obj = (cameraSelector.selectedObject.transform.parent != null) ? cameraSelector.selectedObject.transform.parent.gameObject : null;
            }
            if (true)
            {
                EventTrigger eventTrigger = newItem.GetComponent<EventTrigger>();
                eventTrigger.triggers.Clear();
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers[0].callback.AddListener((data) =>
                {
                    if (obj == null) return;
                    holdingObject = obj;
                    Plugin.LogInfo($"Holding object: {holdingObject.name}");
                });
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers[1].callback.AddListener((data) =>
                {
                    if (obj != null)
                        Plugin.LogInfo($"Entered object: {obj.name}");
                    if (holdingObject != null && holdingObject != obj)
                    {
                        if (obj == null)
                            holdingTarget = holdingObject;
                        else
                            holdingTarget = obj;
                    }
                });
                eventTrigger.triggers.Add(new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit,
                    callback = new EventTrigger.TriggerEvent()
                });
                eventTrigger.triggers[2].callback.AddListener((data) =>
                {
                    holdingTarget = null;
                });
            }
        }

        public static void Log(string str)
        {
            Plugin.LogInfo(str);
        }
    }
}
