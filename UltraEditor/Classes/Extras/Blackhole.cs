namespace BlackholeChaos.Scripts;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Blackhole : MonoBehaviour
{
    public float range = 0;
    public float force = 0f;
    public int difficulty = 1;
    public MeshRenderer meshRendeder, meshRendeder2, meshRendeder3;
    bool inHole = false;
    public bool white = false;
    List<Rigidbody> touchedRigidbodies = new List<Rigidbody>();
    float A = 0;

    public void Create()
    {
        transform.position = Vector3.zero + new Vector3(0, 25, 0);
        transform.position = NewMovement.Instance.transform.position + new Vector3(0, 25, 0);
        difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0);

        SetupVisual();
    }

    void SetupVisual()
    {
        Color c = new Color(0f, 0f, 0f, 0f);
        if (white)
            c = new Color(1f, 1f, 1f, 0f);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        MeshFilter tempMeshFilter = tempSphere.GetComponent<MeshFilter>();
        if (tempMeshFilter != null)
            meshFilter.mesh = tempMeshFilter.sharedMesh;
        Destroy(tempSphere);
        meshRenderer.enabled = false;

        // Black material
        Material mainMat = new Material(Shader.Find("Standard"));
        mainMat.color = c;
        mainMat.SetFloat("_Mode", 3);
        mainMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mainMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mainMat.SetInt("_ZWrite", 0);
        mainMat.DisableKeyword("_ALPHATEST_ON");
        mainMat.EnableKeyword("_ALPHABLEND_ON");
        mainMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mainMat.renderQueue = 3000;
        meshRenderer.material = mainMat;

        transform.localScale = Vector3.one * 2f;

        // Make a sphere
        GameObject child = new GameObject("BlackholeShell");
        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter2 = child.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer2 = child.AddComponent<MeshRenderer>();

        meshFilter2.mesh = Instantiate(meshFilter.mesh);

        Material shellMat = new Material(Shader.Find("Standard"));
        shellMat.color = c;
        shellMat.SetFloat("_Mode", 3);
        shellMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shellMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shellMat.SetInt("_ZWrite", 0);
        shellMat.DisableKeyword("_ALPHATEST_ON");
        shellMat.EnableKeyword("_ALPHABLEND_ON");
        shellMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        shellMat.renderQueue = 3000;
        shellMat.shader = DefaultReferenceManager.Instance.masterShader;

        meshRenderer2.material = shellMat;

        meshRendeder = meshRenderer2;

        // Make a sphere
        child = new GameObject("BlackholeShell");
        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;

        meshFilter2 = child.AddComponent<MeshFilter>();
        meshRenderer2 = child.AddComponent<MeshRenderer>();

        meshFilter2.mesh = Instantiate(meshFilter.mesh);

        shellMat = new Material(Shader.Find("Standard"));
        shellMat.color = c;
        shellMat.SetFloat("_Mode", 3);
        shellMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shellMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shellMat.SetInt("_ZWrite", 0);
        shellMat.DisableKeyword("_ALPHATEST_ON");
        shellMat.EnableKeyword("_ALPHABLEND_ON");
        shellMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        shellMat.renderQueue = 3000;
        shellMat.shader = DefaultReferenceManager.Instance.masterShader;

        meshRenderer2.material = shellMat;

        Mesh oldMesh = Instantiate(meshFilter2.mesh);
        oldMesh.triangles.Reverse();
        oldMesh.RecalculateNormals();
        meshFilter2.mesh = oldMesh;

        meshRendeder3 = meshRenderer2;

        // Make a sphere
        child = new GameObject("BlackholeShell");
        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;

        meshFilter2 = child.AddComponent<MeshFilter>();
        meshRenderer2 = child.AddComponent<MeshRenderer>();

        meshFilter2.mesh = meshFilter.mesh;
        shellMat = new Material(Shader.Find("Standard"));
        shellMat.color = c;
        shellMat.SetFloat("_Mode", 3);
        shellMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shellMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shellMat.SetInt("_ZWrite", 0);
        shellMat.DisableKeyword("_ALPHATEST_ON");
        shellMat.EnableKeyword("_ALPHABLEND_ON");
        shellMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        shellMat.renderQueue = 3000;
        shellMat.shader = Shader.Find("Legacy Shaders/Diffuse");
        if (white)
            shellMat.shader = Shader.Find("Unlit/Texture");

        meshRenderer2.material = shellMat;

        meshRendeder2 = meshRenderer2;
    }

    public void Update()
    {
        if (Time.deltaTime == 0) return;
        if (meshRendeder == null)
            return;
        meshRendeder3.transform.position = Camera.main.transform.position;
        meshRendeder3.enabled = Vector3.Distance(NewMovement.Instance.transform.position, transform.position) <= range;
    }

    public void FixedUpdate()
    {
        if (Time.deltaTime == 0) return;
        if (meshRendeder == null)
            return;
        float m = (((float)difficulty + 1f) / 4f);
        force = force * (1 + Time.fixedDeltaTime / 2000) + Time.fixedDeltaTime * 500 * m;
        range = range * (1 + Time.fixedDeltaTime / 1000) + Time.fixedDeltaTime * 7 * m;

        meshRendeder.transform.localScale = new Vector3(1, 1, 1) * range;
        meshRendeder3.transform.localScale = new Vector3(1f, 1f, 1f);
        meshRendeder2.transform.localScale = new Vector3(1, 1, 1) * range / 5;

        if (Vector3.Distance(NewMovement.Instance.transform.position, transform.position) < range)
        {
            if (Vector3.Distance(NewMovement.Instance.transform.position, transform.position) < range / 5)
            {
                NewMovement.Instance.GetHurt(1000, false);
                StatsManager.Instance.currentCheckPoint = null;
                CheckPointsController.Instance.DisableCheckpoints();
            }
            inHole = true;
        }
        else
        {
            inHole = false;
        }

        // Get all colliders inside the blackhole
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        int colsCount = 0;

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            EnemyIdentifier eid = col.GetComponent<EnemyIdentifier>();
            if (rb != null && rb != this.GetComponent<Rigidbody>())
            {
                colsCount++;
                if (colsCount > 25 && eid == null) continue; // Limits rigidbodies
                Vector3 direction = (transform.position - rb.position).normalized;
                float distance = Vector3.Distance(transform.position, rb.position);

                float finalForce = (white ? -1 : 1) * force / Mathf.Max(distance, 1f);

                if (distance < range / 5f)
                {
                    rb.transform.localScale += 10 * new Vector3(UnityEngine.Random.Range(0f, 0.1f), UnityEngine.Random.Range(0f, 0.2f), UnityEngine.Random.Range(0f, 0.1f)) * Time.fixedDeltaTime;
                    if (!touchedRigidbodies.Contains(rb))
                    {
                        Fat();
                        touchedRigidbodies.Add(rb);
                    }

                    if (distance <= range / 10f && eid == null && NewMovement.Instance.gameObject != rb.gameObject)
                    {
                        Fat();
                        Destroy(rb.gameObject);
                        continue;
                    }
                }

                rb.AddForce(direction * finalForce, ForceMode.Force);
            }


            if (eid != null && Vector3.Distance(transform.position, col.transform.position) < range / 5)
            {
                Fat();
                DamageEnemy(eid);
            }
        }
    }

    void Fat()
    {
        range += 3;
        force += 400;
    }

    public void OnGUI()
    {
        if (inHole)
        {
            float distance = Vector3.Distance(NewMovement.Instance.transform.position, transform.position);
            float maxDistance = range;
            float a = Mathf.Clamp01(5f - (distance / maxDistance)) * 0.6f + 0.1f;

            A = A - (A - a) / 200;
        }
        else
        {
            A = A - (A - 0f) / 20;
        }

        Color guiColor = GUI.color;
        float c = white ? 1f : 0f;
        GUI.color = new Color(c, c, c, A / 2f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = guiColor;
    }

    private void DamageEnemy(EnemyIdentifier eid)
    {
        eid.DeliverDamage(gameObject, Vector3.zero, Vector3.zero, 1000, false);
    }
}