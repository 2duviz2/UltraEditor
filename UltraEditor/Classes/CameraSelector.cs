using System.Collections.Generic;
using UltraEditor.Classes.Canvas;
using UltraEditor.Classes.IO.SaveObjects;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes
{
    public class CameraSelector : MonoBehaviour
    {
        public enum SelectionMode
        {
            Cursor,
            Move,
            Scale,
            Rotate,
        }

        public Camera camera;
        public GameObject selectedObject;
        public Material highlightMaterial;

        SelectionMode _selectionMode = SelectionMode.Cursor;
        public SelectionMode selectionMode 
        { 
            get
            {
                return _selectionMode;
            }
            set
            {
                if (value == SelectionMode.Cursor)
                    DeleteArrows();
                if (value != SelectionMode.Cursor)
                    ClearHover();
                _selectionMode = value;
                ModeButton.UpdateButtons();
            }
        }

        struct MeshEntry
        {
            public Mesh mesh;
            public Matrix4x4 localToRoot;
            public Material mat;
        }

        List<MeshEntry> meshes = new List<MeshEntry>();
        public Material ghostMaterial;
        public Material ghostMaterial2;
        bool objectActive = false;

        void CacheMeshes()
        {
            meshes.Clear();
            if (selectedObject == null) return;

            CacheRecursive(selectedObject.transform, selectedObject.transform);
        }

        void CacheRecursive(Transform t, Transform root)
        {
            var mf = t.GetComponent<MeshFilter>();
            var mr = t.GetComponent<MeshRenderer>();

            Material currentMaterial = ghostMaterial;
            if (t.gameObject.layer == LayerMask.NameToLayer("Invisible")) currentMaterial = ghostMaterial;
            if (!t.gameObject.activeInHierarchy) currentMaterial = ghostMaterial2;

            if (t.GetComponent<PrefabObject>() != null && t.GetComponent<PrefabObject>().PrefabAsset.StartsWith("Assets/Prefabs/Enemies/")) return;

            if (mf && mr && mf.sharedMesh)
            {
                if (!t.gameObject.activeInHierarchy || t.gameObject.layer == LayerMask.NameToLayer("Invisible"))
                {
                    float scaleFactor = 1f;

                    Bounds b = mf.sharedMesh.bounds;
                    Vector3 worldScale = t.lossyScale;
                    Vector3 worldSize = Vector3.Scale(b.size, worldScale);

                    if (Mathf.Max(worldSize.x, worldSize.y, worldSize.z) > 100f && t.GetComponent<SavableObject> == null)
                        scaleFactor = 0.01f;

                    meshes.Add(new MeshEntry
                    {
                        mesh = mf.sharedMesh,
                        localToRoot = root.worldToLocalMatrix * t.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * scaleFactor),
                        mat = currentMaterial
                    });
                }
            }

            var skinned = t.GetComponent<SkinnedMeshRenderer>();
            if (skinned && skinned.sharedMesh)
            {
                if (!t.gameObject.activeInHierarchy || t.gameObject.layer == LayerMask.NameToLayer("Invisible"))
                {
                    var baked = new Mesh();
                    skinned.BakeMesh(baked);

                    float scaleFactor = 1f;
                    Bounds b = baked.bounds;
                    Vector3 worldScale = t.lossyScale;
                    Vector3 worldSize = Vector3.Scale(b.size, worldScale);

                    if (Mathf.Max(worldSize.x, worldSize.y, worldSize.z) > 100f && t.GetComponent<SavableObject> == null)
                        scaleFactor = 0.01f;

                    meshes.Add(new MeshEntry
                    {
                        mesh = baked,
                        localToRoot = root.worldToLocalMatrix * t.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * scaleFactor),
                        mat = currentMaterial
                    });
                }
            }

            for (int i = 0; i < t.childCount; i++)
                CacheRecursive(t.GetChild(i), root);
        }

        static Bounds TransformBounds(Bounds b, Matrix4x4 m)
        {
            Vector3 center = m.MultiplyPoint3x4(b.center);

            Vector3 extents = b.extents;
            Vector3 axisX = m.MultiplyVector(new Vector3(extents.x, 0, 0));
            Vector3 axisY = m.MultiplyVector(new Vector3(0, extents.y, 0));
            Vector3 axisZ = m.MultiplyVector(new Vector3(0, 0, extents.z));

            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds(center, extents * 2f);
        }

        GameObject hoveredObject;
        Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

        private Transform[] moveArrows;
        public bool dragging = false;
        int draggingAxis = -1;
        Vector3 dragStartPos;
        Vector3 objectStartPos, objectStartScale, objectStartEuler;
        (int x, int y) savedMousePos = new (0,0);
        Vector2 realMousePos = Vector2.zero;

        public void Awake()
        {
            if (!camera)
                camera = GetComponent<Camera>();

            highlightMaterial = //CreateGhostMaterial(new Color(1f, 1f, 1f));
                new Material(DefaultReferenceManager.Instance.blankMaterial);
            highlightMaterial.color = new Color(0.5f, 0.5f, 1f);
            ghostMaterial = CreateGhostMaterial(new Color(0.25f, 0.25f, 1f));
            ghostMaterial2 = CreateGhostMaterial(new Color(0.25f, 0.25f, 0.25f));

            ModeButton.UpdateButtons();
        }

        Material CreateGhostMaterial(Color color, float thickness = 30f)
        {
            var shader = BundlesManager.editorBundle.LoadAsset<Shader>("GhostDottedOutline");
            var mat = new Material(shader);

            mat.SetColor("_Color", color);
            mat.SetFloat("_Thickness", 0.02f);
            mat.SetFloat("_DotScale", thickness);
            mat.SetFloat("_DotCutoff", 0f);
            mat.SetFloat("_Offset", 0f);

            return mat;
        }

        public void Update()
        {
            if (Input.GetKeyDown(Plugin.selectCursorKey)) selectionMode = SelectionMode.Cursor;

            if (Input.GetKeyDown(Plugin.selectMoveKey)) selectionMode = SelectionMode.Move;

            if (Input.GetKeyDown(Plugin.selectScaleKey)) selectionMode = SelectionMode.Scale;

            if (Input.GetKeyDown(Plugin.selectRotationKey)) selectionMode = SelectionMode.Rotate;

            if (selectionMode == SelectionMode.Cursor)
                HandleCursorMode();

            if (selectionMode != SelectionMode.Cursor && selectedObject)
                HandleMoveMode();

            if (selectedObject == null || !EditorManager.Instance.IsObjectEditable())
            {
                if (selectionMode != SelectionMode.Cursor)
                    DeleteArrows();
                selectionMode = SelectionMode.Cursor;
            }

            if (selectedObject != null)
            {
                if (objectActive != selectedObject.activeInHierarchy)
                {
                    objectActive = selectedObject.activeInHierarchy;
                    CacheMeshes();
                }
            }
        }

        float timeToUpdate = 0.1f;
        public void LateUpdate()
        {
            highlightMaterial.SetFloat("_Offset", highlightMaterial.GetFloat("_Offset") + Time.unscaledDeltaTime);
            if (selectedObject == null) return;

            Matrix4x4 rootMatrix = selectedObject.transform.localToWorldMatrix;

            ghostMaterial.SetFloat("_Offset", ghostMaterial.GetFloat("_Offset") + Time.unscaledDeltaTime);
            ghostMaterial2.SetFloat("_Offset", ghostMaterial2.GetFloat("_Offset") + Time.unscaledDeltaTime);
            timeToUpdate -= Time.unscaledDeltaTime;
            if (timeToUpdate <= 0)
            {
                timeToUpdate = 0.1f;
                CacheMeshes();
            }

            foreach (var e in meshes)
            {
                if (e.mesh == null) continue;

                Matrix4x4 worldMatrix = rootMatrix * e.localToRoot;

                Bounds worldBounds = TransformBounds(e.mesh.bounds, worldMatrix);

                if (worldBounds.Contains(camera.transform.position))
                    continue;

                Graphics.DrawMesh(
                    e.mesh,
                    worldMatrix,
                    e.mat,
                    0
                );
            }
        }

        void OnDisable()
        {
            ClearHover();
        }

        void HandleCursorMode()
        {
            if (hoveredObject != null && hoveredObject != selectedObject)
            {
                RestoreMaterial(hoveredObject);
                hoveredObject = null;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, camera.cullingMask))
            {
                if (hit.distance > 0.1f)
                {
                    GameObject hitObj = hit.collider.gameObject;
                    if (hitObj != selectedObject)
                    {
                        hoveredObject = hitObj;
                        var renderer = hitObj.GetComponent<Renderer>();
                        if (renderer && (EditorManager.Instance.IsObjectEditable(hitObj) || EditorManager.advancedInspector))
                        {
                            if (!originalMaterials.ContainsKey(hitObj))
                                originalMaterials[hitObj] = renderer.material;

                            renderer.material = highlightMaterial;
                        }
                    }

                    if (Input.GetMouseButtonDown(0))
                        if (Input.GetKey(Plugin.altKey))
                            EditorManager.Instance.SelectObject(hitObj);
                        else
                            if (EditorManager.Instance.IsObjectEditable(hitObj) || EditorManager.advancedInspector)
                                SelectObject(hitObj);
                }
            }
        }

        public void ClearHover()
        {
            if (hoveredObject)
                RestoreMaterial(hoveredObject);
            hoveredObject = null;
        }

        void RestoreMaterial(GameObject obj)
        {
            if (obj.GetComponent<CubeObject>() != null && obj.GetComponent<MaterialChoser>() != null)
            {
                obj.GetComponent<MaterialChoser>().ProcessMaterial(obj.GetComponent<CubeObject>().matType);
                return;
            }

            if (obj && originalMaterials.TryGetValue(obj, out Material original))
            {
                var r = obj.GetComponent<Renderer>();
                if (r)
                    r.material = original;
            }
        }

        public void ClearSelectedMaterial()
        {
            if (selectedObject)
                RestoreMaterial(selectedObject);
        }

        public void UnselectObject()
        {
            if (selectedObject)
                RestoreMaterial(selectedObject);
            selectedObject = null;
            EditorManager.PlayAudio(EditorManager.unselectObject);

            ClearHover();
            if (moveArrows != null)
            {
                foreach (var arrow in moveArrows)
                    Destroy(arrow.gameObject);
                moveArrows = null;
            }
        }

        float scaleMultiplier = 1;
        void HandleMoveMode()
        {
            if (moveArrows == null)
                CreateMoveArrows();

            if (selectedObject.isStatic)
            {
                selectionMode = SelectionMode.Cursor;
                EditorManager.Log("Can't move an static object");
            }

            UpdateMoveArrows();

            Vector3 mousePos = Input.mousePosition;

            if (!dragging)
            {
                int hoveredAxis = -1;
                float minDist = 999f;

                for (int i = 0; i < moveArrows.Length; i++)
                {
                    Vector3 screenPos = camera.WorldToScreenPoint(moveArrows[i].position);
                    float dist = Vector2.Distance(mousePos, screenPos);

                    if (dist < 40f && dist < minDist)
                    {
                        hoveredAxis = i;
                        minDist = dist;
                    }
                }

                if (hoveredAxis != -1)
                {
                    for (int i = 0; i < moveArrows.Length; i++)
                    {
                        var r = moveArrows[i].GetComponent<Renderer>();
                        r.material.color = (i == hoveredAxis)
                            ? r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 1f)
                            : r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 0.5f);
                }

                    if (Input.GetMouseButtonDown(0))
                    {
                        dragging = true;
                        draggingAxis = hoveredAxis;
                        objectStartPos = selectedObject.transform.position;
                        objectStartScale = selectedObject.transform.localScale;
                        objectStartEuler = selectedObject.transform.eulerAngles;
                        dragStartPos = mousePos;
                        scaleMultiplier = moveArrows[0].localScale.y;
                        savedMousePos = MouseController.GetMousePos();
                        realMousePos = mousePos;
                        Cursor.visible = false;
                        Billboard.DeleteAll();
                    }
                }
                else
                {
                    for (int i = 0; i < moveArrows.Length; i++)
                    {
                        var r = moveArrows[i].GetComponent<Renderer>();
                        Color baseColor = (i == 0 ? Color.red : (i == 1 ? Color.green : Color.blue));
                        r.material.color = baseColor;
                    }
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    realMousePos += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                    Vector3 mouseDelta = new Vector3(realMousePos.x, realMousePos.y, 0) - dragStartPos;
                    MouseController.SetCursorPos(savedMousePos.x, savedMousePos.y);
                    float moveSpeed = scaleMultiplier * 0.1f;

                    Vector3 moveDir = Vector3.zero;
                    float delta = 0f;

                    if (draggingAxis == 0) { moveDir = Vector3.right; delta = mouseDelta.x; }
                    if (draggingAxis == 1) { moveDir = Vector3.up; delta = mouseDelta.y; }
                    if (draggingAxis == 2) { moveDir = Vector3.forward; delta = mouseDelta.x; }

                    if (selectionMode == SelectionMode.Move)
                    {
                        Vector3 target = objectStartPos + moveDir * delta * moveSpeed * 3;
                        var s = 0.25f;
                        if (Input.GetKey(Plugin.shiftKey))
                            s = 1;
                        if (Input.GetKey(Plugin.ctrlKey))
                            target = Snap(target, s);
                        selectedObject.transform.position = target;
                    }
                    if (selectionMode == SelectionMode.Scale)
                    {
                        Vector3 target = objectStartScale + moveDir * delta * moveSpeed * 3;
                        var s = 0.25f;
                        if (Input.GetKey(Plugin.shiftKey))
                            s = 1;
                        if (Input.GetKey(Plugin.ctrlKey))
                            target = Snap(target, s);
                        selectedObject.transform.localScale = target;
                    }
                    if (selectionMode == SelectionMode.Rotate)
                    {
                        Vector3 target = objectStartEuler + moveDir * delta * 5;
                        var s = 15f;
                        if (Input.GetKey(Plugin.shiftKey))
                            s = 45;
                        if (Input.GetKey(Plugin.ctrlKey))
                            target = Snap(target, s);
                        selectedObject.transform.eulerAngles = target;
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    dragging = false;
                    draggingAxis = -1;
                    EditorManager.Instance.UpdateInspector();
                    Cursor.visible = true;
                    Billboard.UpdateBillboards();
                }
            }
        }

        public void SelectObject(GameObject obj)
        {
            EditorManager.PlayAudio(EditorManager.selectObject);
            if (selectedObject != null)
                RestoreMaterial(selectedObject);

            selectedObject = obj;
            Debug.Log("[CameraSelector] Selected object: " + obj.name);

            selectionMode = SelectionMode.Move;

            if (moveArrows == null)
                CreateMoveArrows();
            UpdateMoveArrows();

            CacheMeshes();
        }

        void CreateMoveArrows()
        {
            if (moveArrows != null)
                DeleteArrows();

            moveArrows = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arrow.GetComponent<Renderer>().material = new Material(DefaultReferenceManager.Instance.masterShader);
                arrow.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                arrow.GetComponent<Collider>().isTrigger = true;
                arrow.GetOrAddComponent<NavMeshModifier>().ignoreFromBuild = true;
                arrow.name = "MoveArrow_" + i;
                var mat = new Material(Shader.Find("Hidden/Internal-Colored"));
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                mat.SetInt("_ZWrite", 0);
                mat.SetColor("_Color", i == 0 ? Color.red : (i == 1 ? Color.green : Color.blue));
                arrow.GetComponent<Renderer>().material = mat;

                moveArrows[i] = arrow.transform;
            }
        }

        public void DeleteArrows()
        {
            if (moveArrows != null)
            {
                foreach (var arrow in moveArrows)
                    Destroy(arrow.gameObject);
                moveArrows = null;
            }
        }

        public void FocusOnSelected()
        {
            if (selectedObject)
            {
                Vector3 direction = (camera.transform.position - selectedObject.transform.position).normalized;
                camera.transform.position = selectedObject.transform.position + direction * 5f;
            }
        }

        void UpdateMoveArrows()
        {
            if (selectedObject == null || moveArrows == null) return;

            Vector3 pos = selectedObject.transform.position;

            moveArrows[0].position = pos + Vector3.right * moveArrows[0].localScale.y * 2;
            moveArrows[0].rotation = Quaternion.Euler(0, 0, 90);

            moveArrows[1].position = pos + Vector3.up * moveArrows[0].localScale.y * 2;
            moveArrows[1].rotation = Quaternion.identity;

            moveArrows[2].position = pos + Vector3.forward * moveArrows[0].localScale.y * 2;
            moveArrows[2].rotation = Quaternion.Euler(90, 0, 0);

            float distance = Vector3.Distance(camera.transform.position, pos);
            float scaleFactor = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float desiredHeight = scaleFactor * 0.1f;
            float uniformScale = desiredHeight * 0.1f;

            foreach (var arrow in moveArrows)
                arrow.localScale = Vector3.one * uniformScale;
        }

        Vector3 GetMouseWorldPos()
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        Vector3 Snap(Vector3 v, float s = 0.25f)
        {
            v.x = Mathf.Round(v.x / s) * s;
            v.y = Mathf.Round(v.y / s) * s;
            v.z = Mathf.Round(v.z / s) * s;
            return v;
        }
    }
}