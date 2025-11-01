using System.Collections.Generic;
using UnityEngine;

namespace UltraEditor.Classes
{
    public class CameraSelector : MonoBehaviour
    {
        public enum SelectionMode
        {
            Cursor,
            Move
        }

        public Camera camera;
        public GameObject selectedObject;
        public Material highlightMaterial;
        public SelectionMode selectionMode = SelectionMode.Cursor;

        private GameObject hoveredObject;
        private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

        private Transform[] moveArrows;
        public bool dragging = false;
        private int draggingAxis = -1;
        private Vector3 dragStartPos;
        private Vector3 objectStartPos;

        public void Awake()
        {
            if (!camera)
                camera = GetComponent<Camera>();
        }

        public void Update()
        {
            if (Input.GetKeyDown(Plugin.selectCursorKey)) selectionMode = SelectionMode.Cursor;
            if (Input.GetKeyDown(Plugin.selectCursorKey)) DeleteArrows();

            if (Input.GetKeyDown(Plugin.selectMoveKey)) selectionMode = SelectionMode.Move;
            if (Input.GetKeyDown(Plugin.selectMoveKey)) ClearHover();

            if (selectionMode == SelectionMode.Cursor)
                HandleCursorMode();

            if (selectionMode == SelectionMode.Move && selectedObject)
                HandleMoveMode();

            if (selectedObject == null)
            {
                if (selectionMode == SelectionMode.Move)
                    DeleteArrows();
                selectionMode = SelectionMode.Cursor;
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
                        if (renderer)
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
                        dragStartPos = mousePos;
                        scaleMultiplier = moveArrows[0].localScale.y;
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
                    Vector3 mouseDelta = Input.mousePosition - dragStartPos;
                    float moveSpeed = scaleMultiplier * 0.1f;

                    Vector3 moveDir = Vector3.zero;
                    float delta = 0f;

                    if (draggingAxis == 0) { moveDir = Vector3.right; delta = mouseDelta.x; }
                    if (draggingAxis == 1) { moveDir = Vector3.up; delta = mouseDelta.y; }
                    if (draggingAxis == 2) { moveDir = Vector3.forward; delta = mouseDelta.x; }

                    selectedObject.transform.position = objectStartPos + moveDir * delta * moveSpeed;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    dragging = false;
                    draggingAxis = -1;
                    EditorManager.Instance.UpdateInspector();
                }
            }
        }

        public void SelectObject(GameObject obj)
        {
            if (selectedObject != null)
                RestoreMaterial(selectedObject);

            selectedObject = obj;
            Debug.Log("[CameraSelector] Selected object: " + obj.name);

            selectionMode = SelectionMode.Move;

            if (moveArrows == null)
                CreateMoveArrows();
            UpdateMoveArrows();
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

            moveArrows[0].position = pos + Vector3.right * moveArrows[0].localScale.y;
            moveArrows[0].rotation = Quaternion.Euler(0, 0, 90);

            moveArrows[1].position = pos + Vector3.up * moveArrows[0].localScale.y;
            moveArrows[1].rotation = Quaternion.identity;

            moveArrows[2].position = pos + Vector3.forward * moveArrows[0].localScale.y;
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
    }
}