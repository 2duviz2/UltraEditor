using HarmonyLib;
using Newtonsoft.Json;
using plog.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UltraEditor.Classes.IO.SaveObjects;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.ExpressionEvaluator;
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
        public static bool logShit = false;

        static string tempScene;

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
            Dropdown,
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
                if (!cameraSelector.dragging && !editorCamera.GetComponent<CameraMovement>().moving())
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
                if (logShit)
                Plugin.LogInfo($"Released mouse button with " +
                    $"{(holdingObject ? holdingObject.name : "null")} & " +
                    $"{(holdingTarget ? holdingTarget.name : "null")}");

                if (holdingObject != null && holdingTarget != null && holdingObject != holdingTarget)
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

                else if (holdingObject == holdingTarget && holdingObject != null)
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

            if (Plugin.isSelectPressed() && cameraSelector.selectedObject != null)
            {
                SelectObject(cameraSelector.selectedObject);
            }
        }

        public void LateUpdate()
        {
            
        }

        public static TMP_Text MissionNameText = null;
        static NavMeshSurface navMeshSurface;
        static string deleteLevel = "Endless";
        static string[] doNotDelete = new string[] { "MapLoader", "Level Info", "FirstRoom", "OnLevelStart", "StatsManager", "Canvas", "GameController", "Player", "EventSystem(Clone)", "CheatBinds", "PlatformerController(Clone)", "CheckPointsController" };
        public static void DeleteScene(bool force = false)
        {
            if (force || ((SceneHelper.CurrentScene == deleteLevel || StatsManager.Instance.endlessMode) && !StatsManager.Instance.timer))
            {
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (logShit)
                        Plugin.LogInfo($"Trying to detroy {obj.name}");
                    if (!doNotDelete.Contains(obj.name) && !obj.name.StartsWith("MoveArrow_") && (Instance != null ? (obj != Instance.editorCamera.gameObject && obj != Instance.editorCanvas.gameObject && obj != Instance.gameObject && obj != navMeshSurface.gameObject) : true))
                    {
                        if (logShit)
                            Plugin.LogInfo($"Destroyed {obj.name}");
                        Destroy(obj);
                    }
                }

                if (navMeshSurface == null)
                {
                    GameObject navMeshObj = new GameObject("NavMeshSurface");
                    navMeshSurface = navMeshObj.AddComponent<NavMeshSurface>();
                    navMeshSurface.collectObjects = CollectObjects.All;
                    navMeshSurface.BuildNavMesh();
                    if (logShit)
                        Plugin.LogInfo("NavMeshSurface created.");

                    StatsManager.Instance.levelNumber = 0;
                    StatsManager.Instance.endlessMode = false;

                    StockMapInfo.Instance.layerName = "ULTRAEDITOR";
                    StockMapInfo.Instance.layerName = "CUSTOM LEVEL";
                    StockMapInfo.Instance.nextSceneName = "Main Menu";

                    GameObject finalRankPanel = Plugin.Ass<GameObject>("Assets/Prefabs/Player/Player.prefab").transform.GetChild(4).GetChild(1).GetChild(0).GetChild(1).GetChild(0).gameObject;
                    GameObject finishCanvas = NewMovement.Instance.transform.GetChild(4).GetChild(1).GetChild(0).GetChild(1).gameObject;

                    Destroy(finishCanvas.transform.GetChild(0).gameObject);
                    GameObject spawnedRank = Instantiate(finalRankPanel, finishCanvas.transform);
                    StatsManager.Instance.fr = spawnedRank.GetComponent<FinalRank>();
                    spawnedRank.GetComponent<FinalRank>().targetLevelName = "Main Menu";
                    spawnedRank.SetActive(false);
                    finishCanvas.SetActive(true);
                    MissionNameText = spawnedRank.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();

                    GameObject.FindObjectOfType<FinalDoorOpener>(true).startMusic = false;
                    GameObject.FindObjectOfType<FinalDoorOpener>(true).startTimer = true;

                    StatsManager.Instance.timeRanks[0] = int.MaxValue;
                    StatsManager.Instance.timeRanks[1] = int.MaxValue;
                    StatsManager.Instance.timeRanks[2] = int.MaxValue;
                    StatsManager.Instance.timeRanks[3] = int.MaxValue;

                    StatsManager.Instance.killRanks[0] = int.MaxValue;
                    StatsManager.Instance.killRanks[1] = int.MaxValue;
                    StatsManager.Instance.killRanks[2] = int.MaxValue;
                    StatsManager.Instance.killRanks[3] = int.MaxValue;

                    StatsManager.Instance.styleRanks[0] = int.MaxValue;
                    StatsManager.Instance.styleRanks[1] = int.MaxValue;
                    StatsManager.Instance.styleRanks[2] = int.MaxValue;
                    StatsManager.Instance.styleRanks[3] = int.MaxValue;
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
                {
                    editorCamera.transform.position = NewMovement.Instance.transform.position;
                    editorCamera.transform.rotation = NewMovement.Instance.transform.rotation;
                    NewMovement.Instance.gameObject.SetActive(mouseLocked);
                }
                editorCanvas.SetActive(!mouseLocked);
                blocker.SetActive(true);
                cameraSelector.ClearHover();
                cameraSelector.UnselectObject();
                cameraSelector.selectionMode = CameraSelector.SelectionMode.Cursor;

                if (mouseLocked)
                {
                    if (SceneHelper.CurrentScene != "Main Menu")
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    if (SceneHelper.CurrentScene == deleteLevel)
                        RebuildNavmesh(Input.GetKey(KeyCode.N));

                    foreach (var item in FindObjectsOfType<Door>())
                    {
                        var m = item.GetType().GetMethod("GetPos",
                            BindingFlags.Instance | BindingFlags.NonPublic);

                        m.Invoke(item, null);

                    }
                }

                if (!mouseLocked && !string.IsNullOrEmpty(tempScene) && !advancedInspector && SceneHelper.CurrentScene == deleteLevel)
                {
                    StartCoroutine(GoToBackupScene());
                }
                if (mouseLocked && !advancedInspector && SceneHelper.CurrentScene == deleteLevel)
                {
                    tempScene = GetSceneJson();
                }

                Time.timeScale = mouseLocked ? 1f : 0f;
                DisableAlert();

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

            if (NewMovement.Instance != null)
            {
                NewMovement.Instance.gameObject.SetActive(mouseLocked);
                defaultCullingMask = CameraController.Instance.cam.cullingMask;
                ChangeCameraCullingLayers(defaultCullingMask);
                NewMovement.Instance.endlessMode = false;
            }
            Time.timeScale = mouseLocked ? 1f : 0f;

            SetupButtons();
            if (GameObject.FindObjectOfType<NavMeshSurface>() != null)
                EditorVisualizers.RebuildNavMeshVis(GameObject.FindObjectOfType<NavMeshSurface>());

            if (!string.IsNullOrEmpty(tempScene) && !advancedInspector && SceneHelper.CurrentScene == deleteLevel) // load backup level after restart
            {
                StartCoroutine(GoToBackupScene());
            }
        }

        void SetupButtons()
        {
            // File
            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                TryToLoadShit();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                TryToSaveShit();
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                string path = Application.persistentDataPath;
                path = path.Replace("/", "\\"); // make Windows happy
                Process.Start("explorer.exe", $"\"{path}\"");
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(3).GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                DeleteScene(true);
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
                RebuildNavmesh(true);
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
                createFloor(new Vector3(25, 1, 25));
            });

            editorCanvas.transform.GetChild(0).GetChild(4).GetChild(1).GetChild(2).GetChild(3).GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                createFloor(new Vector3(25, 10, 1));
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
                SetAlert("Advanced inspector will remove some autoamtic features of the editor! It's recommended to use this option as a testing resource instead of level making.", "Warning!");
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

        void RebuildNavmesh(bool forceFindNavmesh)
        {
            if (navMeshSurface == null && forceFindNavmesh)
            {
                navMeshSurface = FindObjectOfType<NavMeshSurface>();
            }

            if (navMeshSurface != null)
            {
                if (Input.GetKey(KeyCode.A))
                    navMeshSurface.collectObjects = CollectObjects.All;
                if (Input.GetKey(KeyCode.V))
                    navMeshSurface.collectObjects = CollectObjects.Volume;
                if (Input.GetKey(KeyCode.M))
                    navMeshSurface.collectObjects = CollectObjects.MarkedWithModifier;

                navMeshSurface.BuildNavMesh();
                EditorVisualizers.RebuildNavMeshVis(navMeshSurface);
                if (logShit)
                    Plugin.LogInfo("NavMesh rebuilt.");
            }
            else
            {
                Plugin.LogError("No NavMeshSurface found to rebuild.");
            }
        }

        void SetupVariables()
        {
            inspectorVariables.Clear();

            NewInspectorVariable("localPosition", typeof(Transform));
            NewInspectorVariable("localEulerAngles", typeof(Transform));
            NewInspectorVariable("localScale", typeof(Transform));

            /*NewInspectorVariable("castShadows", typeof(MeshRenderer));
            NewInspectorVariable("receiveShadows", typeof(MeshRenderer));*/

            //NewInspectorVariable("center", typeof(BoxCollider));
            /*NewInspectorVariable("size", typeof(BoxCollider));
            NewInspectorVariable("isTrigger", typeof(BoxCollider));*/

            //NewInspectorVariable("center", typeof(CapsuleCollider));
            /*NewInspectorVariable("radius", typeof(CapsuleCollider));
            NewInspectorVariable("height", typeof(CapsuleCollider));
            NewInspectorVariable("isTrigger", typeof(CapsuleCollider));*/

            /*NewInspectorVariable("drag", typeof(Rigidbody));
            NewInspectorVariable("angularDrag", typeof(Rigidbody));
            NewInspectorVariable("mass", typeof(Rigidbody));
            NewInspectorVariable("useGravity", typeof(Rigidbody));
            NewInspectorVariable("isKinematic", typeof(Rigidbody));*/

            /*NewInspectorVariable("speed", typeof(NavMeshAgent));
            NewInspectorVariable("angularSpeed", typeof(NavMeshAgent));
            NewInspectorVariable("acceleration", typeof(NavMeshAgent));*/

            /*NewInspectorVariable("volume", typeof(AudioSource));
            NewInspectorVariable("pitch", typeof(AudioSource));
            NewInspectorVariable("loop", typeof(AudioSource));
            NewInspectorVariable("spatialize", typeof(AudioSource));
            NewInspectorVariable("spatialBlend", typeof(AudioSource));
            NewInspectorVariable("panStereo", typeof(AudioSource));
            NewInspectorVariable("reverbZoneMix", typeof(AudioSource));
            NewInspectorVariable("dopplerLevel", typeof(AudioSource));
            NewInspectorVariable("priority", typeof(AudioSource));
            NewInspectorVariable("minDistance", typeof(AudioSource));
            NewInspectorVariable("maxDistance", typeof(AudioSource));*/

            /*NewInspectorVariable("speed", typeof(Animator));*/

            /*NewInspectorVariable("ignoreFromBuild", typeof(NavMeshModifier));*/

            NewInspectorVariable("matType", typeof(CubeObject));

            // Enemies
            /*NewInspectorVariable("health", typeof(Zombie));
            NewInspectorVariable("health", typeof(Statue));
            NewInspectorVariable("originalHealth", typeof(Statue));*/

            // Triggers
            /*NewInspectorVariable("timed", typeof(HudMessage));
            NewInspectorVariable("message", typeof(HudMessage));
            NewInspectorVariable("timerTime", typeof(HudMessage));*/

            NewInspectorVariable("enemies", typeof(ActivateArena));
            NewInspectorVariable("onlyWave", typeof(ActivateArena));

            NewInspectorVariable("nextEnemies", typeof(ActivateNextWave));
            NewInspectorVariable("toActivate", typeof(ActivateNextWave));
            NewInspectorVariable("enemyCount", typeof(ActivateNextWave));
            NewInspectorVariable("lastWave", typeof(ActivateNextWave));

            NewInspectorVariable("toActivate", typeof(ActivateObject));
            NewInspectorVariable("toDeactivate", typeof(ActivateObject));
            NewInspectorVariable("canBeReactivated", typeof(ActivateObject));

            NewInspectorVariable("intensity", typeof(Light));
            NewInspectorVariable("range", typeof(Light));
            NewInspectorVariable("type", typeof(Light));

            //NewInspectorVariable("toActivate", typeof(CheckPoint)); i dont think levels will use this as navmesh is client-side baked, but it will be added
            NewInspectorVariable("rooms", typeof(CheckPoint));
            NewInspectorVariable("roomsToInherit", typeof(CheckPoint));

            NewInspectorVariable("notInstakill", typeof(DeathZone));
            NewInspectorVariable("damage", typeof(DeathZone));
            NewInspectorVariable("affected", typeof(DeathZone));

            NewInspectorVariable("calmThemeUrl", typeof(MusicObject));
            NewInspectorVariable("battleThemeUrl", typeof(MusicObject));
        }

        void NewInspectorVariable(string varName, Type parentComponent)
        {
            inspectorVariables.Add(new InspectorVariable
            {
                varName = varName,
                parentComponent = parentComponent
            });
        }

        public GameObject SpawnAsset(string dir, bool isLoading = false)
        {
            GameObject obj = Instantiate(Plugin.Ass<GameObject>(dir));
            obj.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f;

            PrefabObject.Create(obj, dir);

            if (Input.GetKey(Plugin.shiftKey) && cameraSelector.selectedObject != null)
            {
                obj.transform.SetParent(cameraSelector.selectedObject.transform);
                lastSelected = null;
            }
            else
                cameraSelector.SelectObject(obj);

            if (Input.GetKey(Plugin.altKey)) obj.SetActive(false);
            else
            {
                if (dir.StartsWith("Assets/Prefabs/Enemies/") && !isLoading)
                {
                    SetAlert("If you plan on making enemy waves, avoid spawning them active! Hold alt when spawning the enemy to disable it in order to avoid automatic gorezones in editor.", title: "Warning!");
                }
            }

            if (dir == "Assets/Prefabs/Levels/Checkpoint.prefab" && !isLoading)
                SetAlert("You need to assign at least one item in rooms for the checkpoint to work. Checkpoints cannot be modified when loaded from a save.", "Warning!");
            if (dir == "Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab" && !isLoading)
                SetAlert("FinalDoor/FinalDoorOpener must be activated to open the door, it must be activated with a trigger and in this version completing the level will result in an infinite stats screen.", "Warning!");

            if (dir == "Bonus")
                obj.GetComponent<Bonus>().secretNumber = 100000;
            
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
            CubeObject.Create(cube, MaterialChoser.materialTypes.MasterShader);

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

        void createFloor(Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = editorCamera.transform.position + editorCamera.transform.forward * 5f + Vector3.down * 1f;
            cube.transform.localScale = scale;
            cube.layer = LayerMask.NameToLayer("Outdoors");
            cube.tag = "Floor";

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
                if (cameraSelector.selectedObject == null && SceneHelper.CurrentScene == deleteLevel && navMeshSurface != null)
                {
                    if (obj.GetComponent<SavableObject>() == null && !advancedInspector)
                        continue;
                }

                if (obj == editorCamera.gameObject || obj == this.gameObject || obj == editorCanvas.gameObject) continue;

                CreateHierarchyItem(obj);
            }

            UpdateInspector();
        }

        public static List<Type> GetAllMonoBehaviourTypes()
        {
            if (Instance != null && !Instance.advancedInspector)
            {
                List<Type> list = new List<Type>();

                list.Add(typeof(ActivateArena));
                list.Add(typeof(ActivateNextWave));
                list.Add(typeof(ActivateObject));
                list.Add(typeof(DeathZone));
                list.Add(typeof(Light));
                list.Add(typeof(MusicObject));

                return list;
            }

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
        Enum lastEnum = null;
        Type lastEnumType = null;
        object coppiedValue = null;
        Type lastCoppiedType = null;
        List<(string, Type, float)> searchResults = new List<(string, Type, float)>();
        List<(string, float)> sceneResults = new List<(string, float)>();
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
                if (!advancedInspector && cameraSelector.selectedObject.GetComponent<SavableObject>() == null)
                {
                    lastComponents = cameraSelector.selectedObject.GetComponents<Component>();
                    return;
                }
                if (advancedInspector || (cameraSelector.selectedObject.GetComponent<ActivateArena>() == null && cameraSelector.selectedObject.GetComponent<ActivateNextWave>() == null && cameraSelector.selectedObject.GetComponent<ActivateObject>() == null && cameraSelector.selectedObject.GetComponent<DeathZone>() == null))
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
                                    if (c is ActivateObject)
                                    {
                                        
                                    }

                                    if (c is ActivateObject || c is ActivateArena || c is DeathZone || c is Light || c is MusicObject)
                                    {
                                        if (cameraSelector.selectedObject.GetComponent<Collider>() != null)
                                        {
                                            cameraSelector.selectedObject.GetComponent<Collider>().isTrigger = true;
                                            SetAlert("Collider has been set to be a trigger.", "Info!");
                                        }
                                    }


                                    if (c is ActivateNextWave)
                                    {
                                        SetAlert("ActivateNextWave will remove any material from the object when saved, as it's meant to be in empty objects and make every enemy be in the child of this object.", "Advice!");
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
                }

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

                    if (advancedInspector)
                    {
                        CreateInspectorItem(compName, inspectorItemType.RemoveButton).AddListener(() =>
                        {
                            Destroy(component);
                        });
                    }
                    else
                    {
                        CreateInspectorItem(compName, inspectorItemType.None);
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
                                if (!inspectorVariables.Any(iv => iv.varName == prop.Name && iv.parentComponent == type))
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
        string choosing_special = "";
        string choosing_field_name = "";
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
                    SetMemberValue(field, comp, ParseVector3(lastFieldText));

                else if (type == typeof(Vector2))
                    SetMemberValue(field, comp, ParseVector2(lastFieldText));

                else if (type == typeof(Vector4))
                    SetMemberValue(field, comp, ParseVector4(lastFieldText));

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
                    CreateInspectorItem(fieldName, inspectorItemType.Dropdown, value != null ? value.ToString() : "null", value).AddListener(() =>
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

                    CreateInspectorItem("<size=10>" + fieldName + $"[{i}] </size>{displayName}", inspectorItemType.ArrayItem, ((Array)value).GetValue(i)?.ToString() ?? "null", ((Array)value).GetValue(i)).AddListener(() =>
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
                                choosing_field_name = fieldName;
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

                    CreateInspectorItem("<size=10>" + fieldName + $"[{i}] </size>{displayName}", inspectorItemType.ArrayItem, element?.ToString() ?? "null", element).AddListener(() =>
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
                                choosing_field_name = fieldName;
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
                CreateInspectorItem($"<color=red>{fieldName}", inspectorItemType.None, "", null);
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

        public Vector3 ParseVector3(string input)
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
            GameObject dropdown = newItem.transform.GetChild(6).gameObject;

            field.SetActive(false);
            removeButton.SetActive(false);
            button.SetActive(false);
            copyButton.SetActive(false);
            pasteButton.SetActive(false);
            dropdown.SetActive(false);

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

            if (itemType == inspectorItemType.Dropdown)
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
                holdingTarget = null;
            });
        }

        public static string GetIdOfObj(GameObject obj, Vector3? offset = null)
        {
            return obj.name + (offset == null ? obj.transform.position : obj.transform.position + offset).ToString() + obj.transform.eulerAngles.ToString() + obj.transform.lossyScale;
        }

        public string addShit(SavableObject obj)
        {
            string text = "";

            obj.Update();
            text += GetIdOfObj(obj.gameObject);
            text += "\n";
            text += obj.name;
            text += "\n";
            text += obj.gameObject.layer;
            text += "\n";
            text += obj.gameObject.tag;
            text += "\n";
            text += obj.Position.ToString();
            text += "\n";
            text += obj.EulerAngles.ToString();
            text += "\n";
            text += obj.Scale.ToString();
            text += "\n";
            text += obj.gameObject.activeSelf ? 1 : 0;
            text += "\n";
            text += obj.transform.parent != null ? GetIdOfObj(obj.transform.parent.gameObject) : "";
            text += "\n";

            return text;
        }

        public List<string> GetAllScenes()
        {
            string path = Application.persistentDataPath;
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
            string text = GetSceneJson();

            File.WriteAllText(Application.persistentDataPath + $"/{path}.uterus", text);
        }

        public string GetSceneJson()
        {
            string text = "";

            foreach (var obj in GameObject.FindObjectsOfType<CubeObject>(true))
            {
                if (obj.GetComponent<ActivateArena>() != null && obj.GetComponent<Collider>().isTrigger)
                {
                    GameObject ob = obj.gameObject;
                    Destroy(obj.GetComponent<CubeObject>());
                    ArenaObject o = ArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponent<ActivateNextWave>() != null)
                {
                    GameObject ob = obj.gameObject;
                    Destroy(obj.GetComponent<CubeObject>());
                    NextArenaObject o = NextArenaObject.Create(ob);
                    continue;
                }

                if (obj.GetComponent<MusicObject>() != null || obj.GetComponent<Light>() != null || obj.GetComponent<ActivateObject>() != null || obj.GetComponent<CheckpointObject>() != null || obj.GetComponent<DeathZone>() != null)
                {
                    continue;
                }

                text += "? CubeObject ?";
                text += "\n";
                text += addShit(obj);
                text += (int)obj.matType + "\n";
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<PrefabObject>(true))
            {
                if (obj.GetComponent<CheckPoint>() != null) continue;
                text += "? PrefabObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.PrefabAsset + "\n";
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<ArenaObject>(true))
            {
                obj.enemyIds.Clear();
                foreach (var e in obj.GetComponent<ActivateArena>().enemies)
                {
                    obj.addId(GetIdOfObj(e));
                }

                text += "? ArenaObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.GetComponent<ActivateArena>().onlyWave + "\n";
                foreach (var e in obj.enemyIds)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<NextArenaObject>(true))
            {
                obj.enemyIds.Clear();
                obj.toActivateIds.Clear();
                if (obj.GetComponent<ActivateNextWave>().nextEnemies != null)
                    foreach (var e in obj.GetComponent<ActivateNextWave>().nextEnemies)
                    {
                        obj.addEnemyId(GetIdOfObj(e));
                    }
                if (obj.GetComponent<ActivateNextWave>().toActivate != null)
                    foreach (var e in obj.GetComponent<ActivateNextWave>().toActivate)
                    {
                        obj.addToActivateId(GetIdOfObj(e));
                    }

                text += "? NextArenaObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.GetComponent<ActivateNextWave>().lastWave + "\n";
                text += obj.GetComponent<ActivateNextWave>().enemyCount + "\n";
                foreach (var e in obj.enemyIds)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in obj.toActivateIds)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<ActivateObject>(true))
            {
                obj.toActivateIds.Clear();
                obj.toDeactivateIds.Clear();
                foreach (var e in obj.toActivate)
                {
                    obj.addToActivateId(GetIdOfObj(e));
                }
                foreach (var e in obj.toDeactivate)
                {
                    obj.addtoDeactivateId(GetIdOfObj(e));
                }

                text += "? ActivateObject ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.toActivateIds)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in obj.toDeactivateIds)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                text += obj.canBeReactivated.ToString();
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<CheckPoint>(true))
            {
                while (obj.GetComponent<CheckpointObject>() != null)
                    Destroy(obj.GetComponent<CheckpointObject>());
                CheckpointObject co = CheckpointObject.Create(obj.gameObject);

                foreach (var e in obj.rooms)
                {
                    if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                        co.addRoomId(GetIdOfObj(e, new Vector3(-10000, 0, 0)));
                    else
                        co.addRoomId(GetIdOfObj(e));
                }
                foreach (var e in obj.roomsToInherit)
                {
                    co.addRoomToInheritId(GetIdOfObj(e));
                }

                text += "? CheckpointObject ?";
                text += "\n";
                if (co.transform.parent != null && co.transform.parent.GetComponent<CheckpointObject>() != null)
                    text += addShit(co.transform.parent.GetComponent<CheckpointObject>());
                else
                    text += addShit(co);
                foreach (var e in co.rooms)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in co.roomsToInherit)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";

                Destroy(obj.GetComponent<CheckpointObject>());
            }

            foreach (var obj in GameObject.FindObjectsOfType<CheckpointObject>(true))
            {
                if (obj.transform.childCount != 0) continue;

                text += "? CheckpointObject ?";
                text += "\n";
                text += addShit(obj);
                foreach (var e in obj.rooms)
                {
                    text += e + "\n";
                }
                text += "? PASS ?\n";
                foreach (var e in obj.roomsToInherit)
                {
                    text += e + "\n";
                }
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<DeathZone>(true))
            {
                if (obj.GetComponent<SavableObject>() == null || obj.GetComponent<PrefabObject>() != null) continue;
                text += "? DeathZone ?";
                text += "\n";
                text += addShit(obj.gameObject.AddComponent<SavableObject>());
                text += obj.notInstakill.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += obj.damage.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += (int)obj.affected;
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<Light>(true))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                text += "? Light ?";
                text += "\n";
                text += addShit(obj.gameObject.AddComponent<SavableObject>());
                text += obj.intensity.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += obj.range.ToString();
                text += "\n";
                text += "? PASS ?\n";
                text += (int)obj.type;
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            foreach (var obj in GameObject.FindObjectsOfType<MusicObject>(true))
            {
                if (obj.GetComponent<SavableObject>() == null) continue;
                text += "? MusicObject ?";
                text += "\n";
                text += addShit(obj);
                text += obj.calmThemePath;
                text += "\n";
                text += "? PASS ?\n";
                text += obj.battleThemePath;
                text += "\n";
                text += "? END ?";
                text += "\n";
            }

            return text;
        }

        public void TryToLoadShit()
        {
            //UltraEditor.Classes.EditorManager.Instance.Save("new saving system test uwu :3", "testing the new saving system rn", "Bryan_-000-", "newsavetesting", "C:\\Users\\freda\\Downloads\\absolute cinema.jpg", "C:\\Users\\freda\\Music\\fe\\femtanyl - LOVESICK, CANNIBAL! (feat takihasdied).mp3", "Bryan_-000-.ULTRAEDITOR.SaveSystemTest");

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

                sceneResults = sceneResults.OrderByDescending(t => t.Item2).ToList();

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
                        LoadShit(sceneName);
                    }
                }
            });
        }

        void LoadShit(string sceneName)
        {
            string path = Application.persistentDataPath + $"/{sceneName}";

            if (!File.Exists(path))
            {
                Plugin.LogError("Save not found!");
                return;
            }

            string text = File.ReadAllText(path);

            Plugin.LogInfo($"Loading {sceneName}...");
            LoadSceneJson(text);

            if (MissionNameText != null)
            {
                Destroy(MissionNameText.GetComponent<LevelNameFinder>());
                MissionNameText.text = sceneName.Replace(".uterus", "");
                lastLoaded = sceneName;
            }

        }

        void LoadSceneJson(string text)
        {
            float startTime = Time.realtimeSinceStartup;

            int lineIndex = 0;
            int phase = 0;
            bool isInScript = false;
            string scriptType = "";
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
                        Plugin.LogInfo($"Line {lineIndex} {line} {scriptType}");

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
                        workingObject.transform.position = ParseVector3(line);
                    if (lineIndex == 6)
                        workingObject.transform.eulerAngles = ParseVector3(line);
                    if (lineIndex == 7)
                        workingObject.transform.localScale = ParseVector3(line);
                    if (lineIndex == 8)
                        workingObject.gameObject.SetActive(line == "1");
                    if (lineIndex == 9)
                        workingObject.GetComponent<SpawnedObject>().parentID = line;

                    if (lineIndex == 10 && scriptType == "CubeObject")
                        CubeObject.Create(workingObject, (MaterialChoser.materialTypes)Enum.GetValues(typeof(MaterialChoser.materialTypes)).GetValue(int.Parse(line)));

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
                            workingObject.GetComponent<LightObject>().intensity = int.Parse(line);
                        else if (phase == 1)
                            workingObject.GetComponent<LightObject>().range = int.Parse(line);
                        else if (phase == 2)
                            workingObject.GetComponent<LightObject>().type = (LightType)Enum.GetValues(typeof(LightType)).GetValue(int.Parse(line));

                    if (scriptType == "MusicObject" && workingObject.GetComponent<MusicObject>() == null)
                        MusicObject.Create(workingObject);
                    if (lineIndex >= 10 && scriptType == "MusicObject")
                        if (phase == 0)
                            workingObject.GetComponent<MusicObject>().calmThemePath = line;
                        else if (phase == 1)
                            workingObject.GetComponent<MusicObject>().battleThemePath = line;
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
                obj.GetComponent<ArenaObject>()?.createArena();
                obj.GetComponent<NextArenaObject>()?.createArena();
                obj.GetComponent<ActivateObject>()?.createActivator();
                obj.GetComponent<CheckpointObject>()?.createCheckpoint();
                obj.GetComponent<DeathZoneObject>()?.createDeathzone();
                obj.GetComponent<LightObject>()?.createLight();
                obj.GetComponent<MusicObject>()?.createMusic();
            }

            Plugin.LogInfo($"Loading done in {Time.realtimeSinceStartup - startTime} seconds!");

            cameraSelector.selectedObject = null;
        }

        IEnumerator GoToBackupScene()
        {
            DeleteScene(true);
            yield return new WaitForEndOfFrame();
            LoadSceneJson(tempScene);
            yield return new WaitForEndOfFrame();
            SetAlert("Loaded scene backup", "Info!");
        }

        public void SetAlert(string str, string title = "Error!")
        {
            GameObject alert = editorCanvas.transform.GetChild(0).GetChild(10).gameObject;
            alert.GetComponent<Animator>().speed = 0.4f;
            if (title == "Info!")
            alert.GetComponent<Animator>().speed = 1.2f;
            alert.SetActive(false);
            alert.SetActive(true);

            alert.transform.GetChild(0).GetComponent<TMP_Text>().text = title;
            alert.transform.GetChild(1).GetComponent<TMP_Text>().text = str;
        }

        public void DisableAlert()
        {
            GameObject alert = editorCanvas.transform.GetChild(0).GetChild(10).gameObject;
            alert.SetActive(false);
        }

        public static void Log(string str)
        {
            Plugin.LogInfo(str);
        }
    }
}