using System.Collections;
using System.Collections.Generic;
using TJ.IrregularGrid;
using UnityEngine;

public class MeshTextureUpdater : MonoBehaviour
{
    public Texture2D[] splatTextures;
    public float splatSize = 0.1f;
    [SerializeField] private GameObject targetObject; // The GameObject whose position we check (e.g., player)
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    // [SerializeField] private Texture2D mainTex, snowTexture;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject dustCloudPrefab;
    private const int dustCloudPoolSize = 200;
    private const float DustCloudLifetime = 3f;
    private Stack<GameObject> _dustCloudPool = new();
    private Texture2D workingTexture;
    private List<Vector3> splatPoints = new List<Vector3>();
    private Color[] cachedSplatPixels;
    private Color[] cachedTargetPixels;
    private struct Triangle
    {
        public Vector3 v0, v1, v2;
        public Vector2 uv0, uv1, uv2;
        public Bounds bounds;
    }
    private List<Triangle> triangles;
    Coroutine splatCoroutine;

    private void Awake()
    {
        // Initialize cached arrays for splat and target pixels
        int maxSplatSize = Mathf.FloorToInt(splatSize * 128 * 1.25f); // Adjust based on max texture size
        cachedSplatPixels = new Color[maxSplatSize * maxSplatSize];
        cachedTargetPixels = new Color[maxSplatSize * maxSplatSize];

        for (int i = 0; i < dustCloudPoolSize; i++)
        {
            GameObject go = Instantiate(dustCloudPrefab, transform);
            go.SetActive(false);
            _dustCloudPool.Push(go);
        }
    }

    public void UpdateBattlefieldTexture(Texture2D baseTexture)
    {
        meshFilter = targetObject.GetComponent<MeshFilter>();
        meshRenderer = targetObject.GetComponent<MeshRenderer>();

        if (workingTexture != null)
            Destroy(workingTexture);

        try
        {
            workingTexture = CreateReadableTexture(baseTexture);
            meshRenderer.sharedMaterial.SetTexture("_MainTex", workingTexture);
            // meshRenderer.sharedMaterial.SetTexture("_BaseMap ", workingTexture);
            Debug.Log("Working texture initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create readable texture: {e.Message}");
        }

        // Preprocess mesh after setting meshFilter
        PreprocessMesh();
        if (splatCoroutine != null) {
            StopCoroutine(splatCoroutine);
        }
        splatCoroutine = StartCoroutine(ProcessSplats());
    }

    private Texture2D CreateReadableTexture(Texture2D source)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(
            source.width, source.height, 0, RenderTextureFormat.ARGB32
        );
        Graphics.Blit(source, tempRT);
        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        RenderTexture.active = tempRT;
        readableTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readableTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRT);
        return readableTex;
    }

    private void PreprocessMesh()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter or mesh is not assigned! Call UpdateBattlefieldTexture first.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] indices = mesh.triangles;

        if (uvs == null || uvs.Length == 0)
        {
            Debug.LogError("Mesh has no UVs! Splats will not appear correctly.");
            return;
        }

        triangles = new List<Triangle>();
        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 v0 = vertices[indices[i]];
            Vector3 v1 = vertices[indices[i + 1]];
            Vector3 v2 = vertices[indices[i + 2]];
            Bounds bounds = new Bounds(v0, Vector3.zero);
            bounds.Encapsulate(v1);
            bounds.Encapsulate(v2);
            bounds.Expand(0.1f);
            triangles.Add(new Triangle
            {
                v0 = v0, v1 = v1, v2 = v2,
                uv0 = uvs[indices[i]], uv1 = uvs[indices[i + 1]], uv2 = uvs[indices[i + 2]],
                bounds = bounds
            });
        }
        // Debug.Log($"Preprocessed {triangles.Count} triangles. UV sample: {uvs[0]}, {uvs[1]}, {uvs[2]}");
    }

    public void ApplySplatAtPoint(Vector3 worldPoint, Vector2? overrideUV = null)
    {
        splatPoints.Add(worldPoint);
        // Debug.Log($"Added splat point at {worldPoint}. Override UV: {overrideUV}. Total splat points: {splatPoints.Count}");
    }
    public void ExplosionAtPoint(Vector3 worldPoint)
    {
        // Debug.Log($"Explosion at point");
        Instantiate(explosionPrefab, worldPoint, Quaternion.identity);
    }
    public void SpawnDustCloudAt(Vector3 worldPoint)
    {
        if (_dustCloudPool.Count == 0) return;

        // Debug.Log($"Spawning dust cloud at {worldPoint}. Pool size before spawn: {_dustCloudPool.Count}");
        GameObject go = _dustCloudPool.Pop();
        go.transform.position = worldPoint;
        go.SetActive(true);
        StartCoroutine(ReturnToDustPool(go));
    }
    private IEnumerator ReturnToDustPool(GameObject go)
    {
        yield return new WaitForSeconds(DustCloudLifetime);
        go.SetActive(false);
        _dustCloudPool.Push(go);
    }

    private IEnumerator ProcessSplats()
    {
        while (true)
        {
            if (splatPoints.Count > 0 && workingTexture != null)
            {
                int pointsToProcess = Mathf.Min(splatPoints.Count, 5);
                for (int i = 0; i < pointsToProcess; i++)
                {
                    Vector3 worldPoint = splatPoints[0];
                    ApplySingleSplat(worldPoint);
                    splatPoints.RemoveAt(0);
                }
                workingTexture.Apply();
                // Debug.Log("Applied texture changes.");
            }
            yield return null;
        }
    }

    private void ApplySingleSplat(Vector3 worldPoint)
    {
        if (workingTexture == null || meshFilter == null)
        {
            Debug.LogWarning("Cannot apply splat: workingTexture or meshFilter is null.");
            return;
        }

        // Add random offset
        worldPoint += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector2 uv = GetUVAtPoint(localPoint);

        // Debug.Log($"World point: {worldPoint}, Local point: {localPoint}, Computed UV: ({uv.x}, {uv.y})");

        // Log UVs before clamping
        Vector2 rawUV = uv;
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);
        if (rawUV != uv)
        {
            Debug.LogWarning($"UVs clamped from ({rawUV.x}, {rawUV.y}) to ({uv.x}, {uv.y})");
        }

        int centerX = Mathf.FloorToInt(uv.x * workingTexture.width);
        int centerY = Mathf.FloorToInt(uv.y * workingTexture.height);
        float randomSize = Random.Range(0.75f, 1.25f);

        // Normalize splat size
        int referenceTexSize = Mathf.Min(splatTextures[0].width, splatTextures[0].height);
        int splatPixelSize = Mathf.FloorToInt(splatSize * workingTexture.width * randomSize);
        splatPixelSize = Mathf.Clamp(splatPixelSize, 16, referenceTexSize); // Increased min size
        splatPixelSize /= 10;
        int halfSize = splatPixelSize / 2;

        if (splatTextures.Length == 0)
        {
            Debug.LogWarning("No splat textures assigned!");
            return;
        }

        Texture2D splatTexture = splatTextures[Random.Range(0, splatTextures.Length)];
        Color[] splatSourcePixels = splatTexture.GetPixels();

        float texWidthRatio = splatTexture.width / (float)referenceTexSize;
        float texHeightRatio = splatTexture.height / (float)referenceTexSize;
        for (int y = 0; y < splatPixelSize; y++)
        {
            for (int x = 0; x < splatPixelSize; x++)
            {
                int splatX = Mathf.FloorToInt(x * texWidthRatio * (splatTexture.width / (float)splatPixelSize));
                int splatY = Mathf.FloorToInt(y * texHeightRatio * (splatTexture.height / (float)splatPixelSize));
                if (splatX >= 0 && splatX < splatTexture.width && splatY >= 0 && splatY < splatTexture.height)
                {
                    cachedSplatPixels[y * splatPixelSize + x] = splatSourcePixels[splatY * splatTexture.width + splatX];
                }
                else
                {
                    cachedSplatPixels[y * splatPixelSize + x] = Color.clear;
                }
            }
        }

        int startX = Mathf.Clamp(centerX - halfSize, 0, workingTexture.width);
        int startY = Mathf.Clamp(centerY - halfSize, 0, workingTexture.height);
        int width = Mathf.Min(splatPixelSize, workingTexture.width - startX);
        int height = Mathf.Min(splatPixelSize, workingTexture.height - startY);

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning($"Invalid splat region: startX={startX}, startY={startY}, width={width}, height={height}");
            return;
        }

        Color[] basePixels = workingTexture.GetPixels(startX, startY, width, height);
        for (int i = 0; i < basePixels.Length; i++)
        {
            Color splatColor = cachedSplatPixels[i];
            basePixels[i] = Color.Lerp(basePixels[i], splatColor, splatColor.a);
        }

        workingTexture.SetPixels(startX, startY, width, height, basePixels);
        // Debug.Log($"Applied splat at UV ({uv.x}, {uv.y}), pixel ({centerX}, {centerY}), size {splatPixelSize}");
    }

    private Vector2 GetUVAtPoint(Vector3 localPoint)
    {
        float minDistance = float.MaxValue;
        Vector2 closestUV = Vector2.zero;
        bool foundValidTriangle = false;

        if (triangles == null || triangles.Count == 0)
        {
            Debug.LogWarning("No triangles preprocessed! Ensure PreprocessMesh is called.");
            return closestUV;
        }

        foreach (var tri in triangles)
        {
            if (!tri.bounds.Contains(localPoint)) continue;

            Vector3 pointOnTriangle = ClosestPointOnTriangle(tri.v0, tri.v1, tri.v2, localPoint);
            float distance = Vector3.SqrMagnitude(localPoint - pointOnTriangle);

            if (distance < minDistance)
            {
                minDistance = distance;
                foundValidTriangle = true;

                Vector3 edge0 = tri.v1 - tri.v0;
                Vector3 edge1 = tri.v2 - tri.v0;
                Vector3 v0ToPoint = pointOnTriangle - tri.v0;

                float d00 = Vector3.Dot(edge0, edge0);
                float d01 = Vector3.Dot(edge0, edge1);
                float d11 = Vector3.Dot(edge1, edge1);
                float d20 = Vector3.Dot(v0ToPoint, edge0);
                float d21 = Vector3.Dot(v0ToPoint, edge1);
                float denom = d00 * d11 - d01 * d01;

                if (Mathf.Abs(denom) < 1e-6f)
                {
                    Debug.LogWarning("Degenerate triangle detected.");
                    continue;
                }

                float v = (d11 * d20 - d01 * d21) / denom;
                float w = (d00 * d21 - d01 * d20) / denom;
                float u = 1.0f - v - w;

                closestUV = u * tri.uv0 + v * tri.uv1 + w * tri.uv2;
                // Debug.Log($"Barycentric: u={u}, v={v}, w={w}, Triangle UVs: ({tri.uv0.x}, {tri.uv0.y}), ({tri.uv1.x}, {tri.uv1.y}), ({tri.uv2.x}, {tri.uv2.y}) -> Computed: ({closestUV.x}, {closestUV.y})");

                // Normalize UVs to [0,1]
                closestUV.x = (closestUV.x - Mathf.Floor(closestUV.x)) % 1.0f;
                closestUV.y = (closestUV.y - Mathf.Floor(closestUV.y)) % 1.0f;
                if (closestUV.x < 0) closestUV.x += 1.0f;
                if (closestUV.y < 0) closestUV.y += 1.0f;
            }
        }

        if (!foundValidTriangle)
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"No valid triangle found for local point {localPoint}. Using fallback UV.");
            #endif
            closestUV = new Vector2(localPoint.x * 0.5f + 0.5f, localPoint.z * 0.5f + 0.5f);
            closestUV.x = Mathf.Clamp01(closestUV.x);
            closestUV.y = Mathf.Clamp01(closestUV.y);
        }

        return closestUV;
    }

    private Vector3 ClosestPointOnTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 point)
    {
        Vector3 edge0 = v1 - v0;
        Vector3 edge1 = v2 - v0;
        Vector3 v0ToPoint = point - v0;

        float d00 = Vector3.Dot(edge0, edge0);
        float d01 = Vector3.Dot(edge0, edge1);
        float d11 = Vector3.Dot(edge1, edge1);
        float d20 = Vector3.Dot(v0ToPoint, edge0);
        float d21 = Vector3.Dot(v0ToPoint, edge1);
        float denom = d00 * d11 - d01 * d01;

        if (Mathf.Abs(denom) < 1e-6f)
        {
            return v0;
        }

        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        if (u >= 0 && v >= 0 && w >= 0)
        {
            return v0 + v * edge0 + w * edge1;
        }

        Vector3 closest = v0;
        float minDist = Vector3.SqrMagnitude(point - v0);

        Vector3[] edges = { edge0, v1 - v2, v2 - v0 };
        Vector3[] starts = { v0, v1, v2 };
        for (int i = 0; i < 3; i++)
        {
            Vector3 start = starts[i];
            Vector3 edge = edges[i];
            float t = Mathf.Clamp01(Vector3.Dot(point - start, edge) / Vector3.Dot(edge, edge));
            Vector3 candidate = start + t * edge;
            float dist = Vector3.SqrMagnitude(point - candidate);
            if (dist < minDist)
            {
                minDist = dist;
                closest = candidate;
            }
        }

        return closest;
    }

    // #if UNITY_EDITOR
    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;
    //         if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == meshFilter.gameObject)
    //         {
    //             // Debug.Log($"hit at {hit.point}, UV: {hit.textureCoord}");
    //             ApplySplatAtPoint(hit.point, hit.textureCoord);
    //         }
    //     }
    //     // else if (targetObject != null && Input.GetKeyDown(KeyCode.Space))
    //     // {
    //     //     Vector3 closestPoint = FindClosestPointOnMesh(targetObject.transform.position);
    //     //     ApplySplatAtPoint(closestPoint);
    //     // }
    // }
    // #endif

    // private Vector3 FindClosestPointOnMesh(Vector3 targetPosition)
    // {
    //     Vector3 localTargetPos = transform.InverseTransformPoint(targetPosition);
    //     Vector3 closestPoint = Vector3.zero;
    //     float minDistance = float.MaxValue;

    //     if (triangles == null || triangles.Count == 0)
    //     {
    //         Debug.LogWarning("No triangles available for closest point calculation.");
    //         return targetPosition;
    //     }

    //     foreach (var tri in triangles)
    //     {
    //         Vector3 pointOnTriangle = ClosestPointOnTriangle(tri.v0, tri.v1, tri.v2, localTargetPos);
    //         float distance = Vector3.SqrMagnitude(localTargetPos - pointOnTriangle);
    //         if (distance < minDistance)
    //         {
    //             minDistance = distance;
    //             closestPoint = pointOnTriangle;
    //         }
    //     }

    //     Debug.Log($"Closest point: {transform.TransformPoint(closestPoint)} for target: {targetPosition}");
    //     return transform.TransformPoint(closestPoint);
    // }

    private void OnDestroy()
    {
        if (workingTexture != null)
            Destroy(workingTexture);
        _dustCloudPool.Clear();
    }
}