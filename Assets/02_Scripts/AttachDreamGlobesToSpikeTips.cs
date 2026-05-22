using UnityEngine;

public class AttachDreamGlobesToSpikeTips : MonoBehaviour
{
    [Header("References")]
    public Transform pointRoot;
    public GameObject dreamGlobePrefab;

    [Header("Spline Object Connection")]
    public SplineObjectConnectToSelectedGlobe splineObjectConnector;

    [Header("Texture Materials")]
    public Material baseMaterial;          // 기본 머티리얼. 예: Dream.Full
    public Texture2D[] globeTextures;      // 여기에 텍스처 27개 넣기
    public string texturePropertyName = "_MainTex";// URP Lit 기준

    [Header("Appear Delay")]
    public float appearDelay = 1.0f;

    [Header("Placement")]
    public Vector3 localPositionOffset = Vector3.zero;
    public Vector3 localRotationEuler = Vector3.zero;

    [Header("DreamGlobe Prefab Scale")]
    public Vector3 localScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Dome Center Distance")]
    public Transform domeCenter;
    public float visibleRadius = 1.7f;
    public float shrinkStartRadius = 4.5f;
    public float hiddenRadius = 5.1f;

    [Header("Globe Scale")]
    public float minGlobeScale = 0f;
    public float maxGlobeScale = 1f;
    public float shrinkSmoothSpeed = 8f;

    [Header("Click Detach")]
    public AlembicSyncPlayer alembicSyncPlayer;
    public bool detachOnClick = true;
    public bool hideOtherGlobesOnClick = true;

    [Header("SmallEgo Material Target")]
    public Renderer[] smallEgoTargetRenderers;

    [Header("Spline Growth")]
    public GrowthRateController splineGrowthController;
    public float splineGrowthDelay = 1.5f;

    [Header("Options")]
    public bool attachOnStart = true;
    public bool clearExistingGlobes = true;
    public string globeNamePrefix = "DreamGlobe_";

    void Start()
    {
        if (attachOnStart)
        {
            AttachDreamGlobes();
        }
    }

    [ContextMenu("Attach Dream Globes")]
    public void AttachDreamGlobes()
    {
        if (pointRoot == null)
        {
            Debug.LogError("Point Root가 비어있습니다.");
            return;
        }

        if (dreamGlobePrefab == null)
        {
            Debug.LogError("DreamGlobe Prefab이 비어있습니다.");
            return;
        }

        if (domeCenter == null)
        {
            Debug.LogWarning("Dome Center가 비어있습니다. Globe 크기 변화가 작동하지 않습니다.");
        }

        if (alembicSyncPlayer == null)
        {
            Debug.LogWarning("Alembic Sync Player가 비어있습니다. 클릭해도 애니메이션 정지가 안 됩니다.");
        }

        if (baseMaterial == null)
        {
            Debug.LogWarning("Base Material이 비어있습니다. 텍스처 머티리얼 생성이 안 될 수 있습니다.");
        }

        if (globeTextures == null || globeTextures.Length == 0)
        {
            Debug.LogWarning("Globe Textures가 비어있습니다.");
        }

        Transform[] allChildren = pointRoot.GetComponentsInChildren<Transform>(true);

        int index = 0;

        foreach (Transform spikeTip in allChildren)
        {
            if (!spikeTip.name.StartsWith("SpikeTip"))
                continue;

            if (clearExistingGlobes)
            {
                for (int i = spikeTip.childCount - 1; i >= 0; i--)
                {
                    Transform child = spikeTip.GetChild(i);

                    if (child.name.StartsWith(globeNamePrefix))
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                            DestroyImmediate(child.gameObject);
                        else
                            Destroy(child.gameObject);
#else
                        Destroy(child.gameObject);
#endif
                    }
                }
            }

            GameObject globe = Instantiate(dreamGlobePrefab, spikeTip);
            globe.name = globeNamePrefix + (index + 1);

            globe.transform.localPosition = localPositionOffset;
            globe.transform.localRotation = Quaternion.Euler(localRotationEuler);
            globe.transform.localScale = localScale;

            ApplyTextureMaterial(globe, index);

            DreamGlobeScaleByCenterDistance scaler =
                globe.GetComponent<DreamGlobeScaleByCenterDistance>();

            if (scaler == null)
            {
                scaler = globe.AddComponent<DreamGlobeScaleByCenterDistance>();
            }

            scaler.domeCenter = domeCenter;
            scaler.visibleRadius = visibleRadius;
            scaler.shrinkStartRadius = shrinkStartRadius;
            scaler.hiddenRadius = hiddenRadius;
            scaler.appearDelay = appearDelay;
            scaler.minScale = minGlobeScale;
            scaler.maxScale = maxGlobeScale;
            scaler.shrinkSmoothSpeed = shrinkSmoothSpeed;

            DreamGlobeClickDetach clickDetach =
                globe.GetComponent<DreamGlobeClickDetach>();

            if (clickDetach == null)
            {
                clickDetach = globe.AddComponent<DreamGlobeClickDetach>();
            }

            clickDetach.alembicSyncPlayer = alembicSyncPlayer;
            clickDetach.pointRoot = pointRoot;
            clickDetach.detachOnClick = detachOnClick;
            clickDetach.hideOtherGlobesOnClick = hideOtherGlobesOnClick;
            clickDetach.globeNamePrefix = globeNamePrefix;
            clickDetach.targetRenderers = smallEgoTargetRenderers;
            clickDetach.splineObjectConnector = splineObjectConnector;
            clickDetach.splineGrowthController = splineGrowthController;
            clickDetach.splineGrowthDelay = splineGrowthDelay;

            index++;
        }

        Debug.Log($"DreamGlobe {index}개 생성 완료");
    }

    private void ApplyTextureMaterial(GameObject target, int index)
    {
        if (baseMaterial == null)
        {
            Debug.LogWarning("Base Material is null.");
            return;
        }

        if (globeTextures == null || globeTextures.Length == 0)
        {
            Debug.LogWarning("Globe Textures is empty.");
            return;
        }

        Texture2D texture = globeTextures[index % globeTextures.Length];

        if (texture == null)
        {
            Debug.LogWarning($"Texture at index {index} is null.");
            return;
        }

        Material newMaterial = new Material(baseMaterial);
        newMaterial.name = $"DreamGlobe_Mat_{index + 1}_{texture.name}";

        int mainTexID = Shader.PropertyToID("_MainTex");
        int baseMapID = Shader.PropertyToID("_BaseMap");

        bool assigned = false;

        if (newMaterial.HasProperty(mainTexID))
        {
            newMaterial.SetTexture(mainTexID, texture);
            assigned = true;
        }

        if (newMaterial.HasProperty(baseMapID))
        {
            newMaterial.SetTexture(baseMapID, texture);
            assigned = true;
        }

        if (!assigned)
        {
            Debug.LogWarning(
                $"{newMaterial.name} has no _MainTex or _BaseMap property. Check Shader Graph Reference name."
            );
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            // sharedMaterial 말고 material로 강제 인스턴스 할당
            r.material = newMaterial;
        }

        Debug.Log(
            $"{target.name} assigned material {newMaterial.name} with texture {texture.name}"
        );
    }
}