using UnityEngine;

public class SpikeGlobeController : MonoBehaviour
{
    [Header("References")]
    public MeshRenderer innerRenderer;
    public Transform innerTransform;
    public GameObject dreamGlobePrefab;

    private Vector3[] spikeSurfacePointsOS;
    private Mesh innerMesh;

    [Header("Globe Settings")]
    public int globeCount = 21;
    public float surfaceRadius = 0f;
    public float minScale = 0.02f;
    public float maxScale = 0.45f;
    public float appearThreshold = 0.05f;

    [Header("Globe Materials")]
    public Material[] globeMaterials;
    public bool cycleMaterials = true;

    [Header("Optional Offset")]
    public float tipOffset = 0.0f;

    private Transform[] globes;

    private Material mat;

    void Start()
    {
        if (innerRenderer == null)
        {
            Debug.LogError("Inner Renderer is missing.");
            enabled = false;
            return;
        }

        if (innerTransform == null)
        {
            innerTransform = innerRenderer.transform;
        }

        if (dreamGlobePrefab == null)
        {
            Debug.LogError("Dream Globe Prefab is missing.");
            enabled = false;
            return;
        }

        mat = innerRenderer.sharedMaterial;

        if (surfaceRadius <= 0f)
        {
            MeshFilter mf = innerRenderer.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Bounds b = mf.sharedMesh.bounds;
                surfaceRadius = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
            }
            else
            {
                surfaceRadius = 10f;
            }
        }

        CacheInnerMesh();
        CacheSpikeSurfacePoints();

        CreateGlobes();
    }

    void Update()
    {
        if (mat == null || globes == null) return;

        float syncedTime = Time.time;
        mat.SetFloat("_SyncedTime", syncedTime);

        Vector4 targetCenter = mat.GetVector("_CenterOS");
        Vector4 placementCenter = mat.GetVector("_PlacementCenterOS");

        float spikeCount = mat.GetFloat("_SpikeCount");
        float spikeDepthRatio = mat.GetFloat("_SpikeDepthRatio");
        float spikeRadius = mat.GetFloat("_SpikeRadius");
        float spikeSharpness = mat.GetFloat("_SpikeSharpness");

        float speedMin = mat.GetFloat("_SpeedMin");
        float speedMax = mat.GetFloat("_SpeedMax");
        float pulsePower = mat.GetFloat("_PulsePower");
        float minimumPulse = mat.GetFloat("_MinimumPulse");
        float debugShowAll = mat.GetFloat("_DebugShowAll");
        float heightJitter = mat.GetFloat("_HeightJitter");
        float angleJitter = mat.GetFloat("_AngleJitter");

        float tipPull = mat.GetFloat("_TipPull");
        float deformMinZ = mat.GetFloat("_DeformMinZ");
        float deformFade = mat.GetFloat("_DeformFade");

        int count = Mathf.Min(globeCount, Mathf.RoundToInt(spikeCount), globes.Length);

        for (int i = 0; i < globes.Length; i++)
        {
            if (i >= count)
            {
                globes[i].gameObject.SetActive(false);
                continue;
            }

            Vector3 centerDir = GetSpikeCenterDir(i, heightJitter, angleJitter);

            float fi = i + 1f;
            float speed = Mathf.Lerp(speedMin, speedMax, Hash(fi * 31.719f));
            float phase = Hash(fi * 91.137f) * Mathf.PI * 2f;

            float p = Pulse01(syncedTime, speed, phase, pulsePower);
            p = Mathf.Lerp(p, 1f, debugShowAll);
            p = Mathf.Max(p, minimumPulse);

            Vector3 placement = new Vector3(
                placementCenter.x,
                placementCenter.y,
                placementCenter.z
            );

            Vector3 target = new Vector3(
                targetCenter.x,
                targetCenter.y,
                targetCenter.z
            );

        Vector3 originalPosOS = FindClosestSurfacePointOS(centerDir, placement);

        // shader와 동일하게 placement 기준 좌표 계산
        Vector3 placementPos = originalPosOS - placement;
        float domeRadius = placementPos.magnitude;
        Vector3 vertexDir = placementPos.normalized;

        // shader와 동일한 influence 계산
        float d = Vector3.Distance(vertexDir, centerDir);
        float influence = Mathf.Clamp01(1f - d / spikeRadius);
        influence = Mathf.Pow(influence, spikeSharpness);

        // shader와 동일한 bottomMask / weight 계산
        float bottomMask = SmoothStep(deformMinZ, deformMinZ + deformFade, vertexDir.z);
        float weight = influence * p * tipPull * bottomMask;

        // shader와 동일한 spikeSurfacePoint / spikeTipPoint 계산
        Vector3 spikeSurfacePointOS = placement + centerDir * domeRadius;

        Vector3 spikeTipPointOS = Vector3.Lerp(
            spikeSurfacePointOS,
            target,
            spikeDepthRatio * p
        );

        // shader의 finalPos = lerp(finalPos, spikeTipPoint, weight)와 동일하게 계산
        Vector3 visibleTipOS = Vector3.Lerp(
            originalPosOS,
            spikeTipPointOS,
            weight
        );

        // 구를 살짝 표면 쪽으로 빼고 싶을 때만 보정
        Vector3 outwardDir = (spikeSurfacePointOS - target).normalized;
        visibleTipOS += outwardDir * tipOffset;

        Vector3 worldPos = innerTransform.TransformPoint(visibleTipOS);

        Transform globe = globes[i];

        // p가 커질수록 globe도 같이 커짐
        float globeP = Mathf.InverseLerp(appearThreshold, 1f, p);
        globeP = SmoothStep(0f, 1f, globeP);

        globe.gameObject.SetActive(globeP > 0.01f);
        globe.position = worldPos;

        float s = Mathf.Lerp(minScale, maxScale, globeP);
        globe.localScale = Vector3.one * s;
        }
    }

    private void CacheInnerMesh()
    {
        MeshFilter mf = innerRenderer.GetComponent<MeshFilter>();

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("Inner MeshFilter or Mesh is missing.");
            enabled = false;
            return;
        }

        innerMesh = mf.sharedMesh;
    }

    private void CacheSpikeSurfacePoints()
    {
        spikeSurfacePointsOS = new Vector3[globeCount];

        float heightJitter = mat.GetFloat("_HeightJitter");
        float angleJitter = mat.GetFloat("_AngleJitter");

        Vector4 placementCenter = mat.GetVector("_PlacementCenterOS");
        Vector3 placement = new Vector3(
            placementCenter.x,
            placementCenter.y,
            placementCenter.z
        );

        Vector3[] vertices = innerMesh.vertices;

        for (int i = 0; i < globeCount; i++)
        {
            Vector3 centerDir = GetSpikeCenterDir(i, heightJitter, angleJitter);

            float bestDot = -999f;
            Vector3 bestVertex = Vector3.zero;

            for (int v = 0; v < vertices.Length; v++)
            {
                Vector3 localPos = vertices[v];
                Vector3 dir = (localPos - placement).normalized;

                float dot = Vector3.Dot(dir, centerDir);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestVertex = localPos;
                }
            }

            spikeSurfacePointsOS[i] = bestVertex;
        }
    }

    private void CreateGlobes()
    {
        globes = new Transform[globeCount];

        for (int i = 0; i < globeCount; i++)
        {
            GameObject g = Instantiate(dreamGlobePrefab, transform);
            g.name = $"DreamGlobe_Spike_{i + 1:00}";
            globes[i] = g.transform;

            ApplyMaterialToGlobe(g, i);
        }
    }

    private Vector3 FindClosestSurfacePointOS(Vector3 centerDir, Vector3 placement)
{
    if (innerMesh == null)
        return Vector3.zero;

    Vector3[] vertices = innerMesh.vertices;

    float bestDot = -999f;
    Vector3 bestVertex = Vector3.zero;

    for (int v = 0; v < vertices.Length; v++)
    {
        Vector3 localPos = vertices[v];
        Vector3 dir = (localPos - placement).normalized;

        float dot = Vector3.Dot(dir, centerDir);

        if (dot > bestDot)
        {
            bestDot = dot;
            bestVertex = localPos;
        }
    }

    return bestVertex;
}

    private void ApplyMaterialToGlobe(GameObject globe, int index)
    {
        if (globeMaterials == null || globeMaterials.Length == 0)
            return;

        int materialIndex;

        if (cycleMaterials)
        {
            materialIndex = index % globeMaterials.Length;
        }
        else
        {
            materialIndex = Mathf.Min(index, globeMaterials.Length - 1);
        }

        Material selectedMaterial = globeMaterials[materialIndex];

        if (selectedMaterial == null)
            return;

        Renderer[] renderers = globe.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.material = selectedMaterial;
        }
    }

    private Vector3 GetSpikeCenterDir(int index, float heightJitter, float angleJitter)
    {
        if (index == 0)
        {
            return Vector3.forward;
        }

        if (index < 9)
        {
            int ringIndex = index - 1;
            float count = 8f;

            float baseTheta = ((float)ringIndex / count) * Mathf.PI * 2f;
            float angleOffset = (Hash(index * 17.13f) - 0.5f) * angleJitter;
            float theta = baseTheta + angleOffset;

            float baseZ = 0.58f;
            float zOffset = (Hash(index * 41.91f) - 0.5f) * heightJitter;
            float z = Mathf.Clamp01(baseZ + zOffset);

            float r = Mathf.Sqrt(Mathf.Clamp01(1f - z * z));

            return new Vector3(
                Mathf.Cos(theta) * r,
                Mathf.Sin(theta) * r,
                z
            ).normalized;
        }
        else
        {
            int ringIndex = index - 9;
            float count = 12f;

            float baseTheta = (((float)ringIndex / count) * Mathf.PI * 2f) + 0.261799f;
            float angleOffset = (Hash(index * 23.77f) - 0.5f) * angleJitter;
            float theta = baseTheta + angleOffset;

            float baseZ = 0.18f;
            float zOffset = (Hash(index * 59.37f) - 0.5f) * heightJitter * 1.4f;
            float z = Mathf.Clamp(baseZ + zOffset, 0.05f, 0.38f);

            float r = Mathf.Sqrt(Mathf.Clamp01(1f - z * z));

            return new Vector3(
                Mathf.Cos(theta) * r,
                Mathf.Sin(theta) * r,
                z
            ).normalized;
        }
    }

    private float Pulse01(float time, float speed, float phase, float pulsePower)
    {
        float s = Mathf.Sin(time * speed + phase) * 0.5f + 0.5f;
        s = SmoothStep(0f, 1f, s);
        return Mathf.Pow(s, pulsePower);
    }

    private float SmoothStep(float edge0, float edge1, float x)
    {
        x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return x * x * (3f - 2f * x);
    }

    private float Hash(float n)
    {
        return Frac(Mathf.Sin(n) * 43758.5453123f);
    }

    private float Frac(float v)
    {
        return v - Mathf.Floor(v);
    }
}