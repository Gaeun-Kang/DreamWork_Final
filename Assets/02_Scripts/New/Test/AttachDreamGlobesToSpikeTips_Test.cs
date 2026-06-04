using INab.Dissolve;
using UnityEngine;

public class AttachDreamGlobesToSpikeTips_Test : MonoBehaviour
{
    [Header("References")]
    public Transform pointRoot;
    public Dissolver mainDissolver;
    public GameObject dreamGlobePrefab;

    [Header("Spline Object Connection")]
    public SplineObjectConnectToSelectedGlobe splineObjectConnector;

    [Header("Texture Materials")]
    public Material baseMaterial;
    public Texture2D[] globeTextures;
    public string texturePropertyName = "_MainTex";

    [Header("Appear Delay")]
    public float appearDelay = 1.0f;

    [Header("Placement")]
    public Vector3 localPositionOffset = Vector3.zero;
    public Vector3 localRotationEuler = Vector3.zero;

    [Header("DreamGlobe Prefab Scale")]
    public Vector3 localScale = new Vector3(0.5f, 0.5f, 0.5f);

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
      /*  if (attachOnStart)
            AttachDreamGlobes();
   */
        }

    //Dissolve 진행 후에 동적 생성 


    [ContextMenu("Attach Dream Globes")]
    public void AttachDreamGlobes()
    {


        if (pointRoot == null) { Debug.LogError("Point Root가 비어있습니다."); return; }
        if (dreamGlobePrefab == null) { Debug.LogError("DreamGlobe Prefab이 비어있습니다."); return; }

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
                    if (!child.name.StartsWith(globeNamePrefix)) continue;
#if UNITY_EDITOR
                    if (!Application.isPlaying) DestroyImmediate(child.gameObject);
                    else Destroy(child.gameObject);
#else
                    Destroy(child.gameObject);
#endif
                }
            }

            GameObject globe = Instantiate(dreamGlobePrefab, spikeTip);
            globe.name = globeNamePrefix + (index + 1);
            globe.transform.localPosition = localPositionOffset;
            globe.transform.localRotation = Quaternion.Euler(localRotationEuler);
            globe.transform.localScale = localScale;

            ApplyTextureMaterial(globe, index);

            // DreamGlobeScaleByCenterDistance 비활성화 (거리 기반 크기 변화 차단)
            DreamGlobeScaleByCenterDistance existingScaler =
                globe.GetComponent<DreamGlobeScaleByCenterDistance>();
            if (existingScaler != null)
                existingScaler.enabled = false;

            DreamGlobeClickDetach clickDetach = globe.GetComponent<DreamGlobeClickDetach>();
            if (clickDetach == null)
                clickDetach = globe.AddComponent<DreamGlobeClickDetach>();

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

        Debug.Log($"[TEST] DreamGlobe {index}개 생성 완료 (거리 기반 Scale 비적용)");
        
    }

    private void ApplyTextureMaterial(GameObject target, int index)
    {
        if (baseMaterial == null) { Debug.LogWarning("Base Material is null."); return; }
        if (globeTextures == null || globeTextures.Length == 0) { Debug.LogWarning("Globe Textures is empty."); return; }

        Texture2D texture = globeTextures[index % globeTextures.Length];
        if (texture == null) { Debug.LogWarning($"Texture at index {index} is null."); return; }

        Material newMaterial = new Material(baseMaterial);
        newMaterial.enableInstancing = true; //추가한 부분 
        newMaterial.name = $"DreamGlobe_Mat_{index + 1}_{texture.name}";

        int mainTexID = Shader.PropertyToID("_MainTex");
        int baseMapID = Shader.PropertyToID("_BaseMap");
        bool assigned = false;

        if (newMaterial.HasProperty(mainTexID)) { newMaterial.SetTexture(mainTexID, texture); assigned = true; }
        if (newMaterial.HasProperty(baseMapID)) { newMaterial.SetTexture(baseMapID, texture); assigned = true; }

        if (!assigned)
            Debug.LogWarning($"{newMaterial.name} has no _MainTex or _BaseMap property.");

        foreach (Renderer r in target.GetComponentsInChildren<Renderer>(true))
            r.material = newMaterial;

        Debug.Log($"{target.name} → {newMaterial.name} / {texture.name}");
    }
}
