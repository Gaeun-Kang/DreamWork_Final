using INab.Dissolve;
using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AttachDreamGlobesToSpikeTips_Test : MonoBehaviour
{
    [Header("References")]
    public Transform pointRoot;
    public Dissolver mainDissolver;
    public EmotionParticlePlayer emotionParticlePlayer;
    public GameObject dreamGlobePrefab;

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

    [Header("Dome Center Distance")]
    public Transform domeCenter;
    public float visibleRadius = 1.7f;
    public float shrinkStartRadius = 4.5f;
    public float hiddenRadius = 5.1f;

    [Header("Globe Scale")]
    public float minGlobeScale = 0.5f; 
    public float maxGlobeScale = 1f;
    public float shrinkSmoothSpeed = 8f;

    [Header("SplineConnectToEgo")]
    public SplineConnectToEgo splineConnectToEgo;

    [Header("Spline Growth")]
    public GrowthRateController splineGrowthController;
    public float splineGrowthDelay = 1.5f;


    [Header("Click Detach")]
    public AlembicSyncPlayer alembicSyncPlayer;
    public bool detachOnSelected = true;
    public bool hideOtherGlobesOnClick = true;

    [Header("Options")]
    public bool attachOnStart = true;
    public bool clearExistingGlobes = true;
    public string globeNamePrefix = "DreamGlobe_";
    private bool _isAttaching;


    //Dissolve 진행 후에 동적 생성으로 테스트 
    //애니메이션 기반 scale 적용  


    [ContextMenu("Attach Dream Globes")]
    public void AttachDreamGlobes()
    {

        if (_isAttaching)
        {
            Debug.LogWarning("[TEST] AttachDreamGlobes 이미 실행 중 - 중복 호출 차단됨");
            return;
        }

        _isAttaching = true;

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

            DreamGlobeClickDetach_Test clickDetach =
                globe.GetComponent<DreamGlobeClickDetach_Test>();

            if (clickDetach == null)
            {
                clickDetach = globe.AddComponent<DreamGlobeClickDetach_Test>();
            }

            clickDetach.alembicSyncPlayer = alembicSyncPlayer;
            clickDetach.pointRoot = pointRoot;
            clickDetach.hideOtherGlobesOnClick = hideOtherGlobesOnClick;
            clickDetach.globeNamePrefix = globeNamePrefix;
            clickDetach.splineConnectToEgo = splineConnectToEgo;
            clickDetach.splineGrowthController = splineGrowthController;
            clickDetach.splineGrowthDelay = splineGrowthDelay;

            GameObject capturedGlobe = globe;
            StartCoroutine(ReinitializeISDKComponents(capturedGlobe));
            index++;
        }

        Debug.Log($"[TEST] DreamGlobe {index}개 생성 완료 (거리 기반 Scale 비적용)");

    }

    private IEnumerator ReinitializeISDKComponents(GameObject globe)
    {
        yield return new WaitForSeconds(0.1f);

        if (globe == null) yield break;

        Transform interactionChild = globe.transform.Find("ISDK_RayGrabInteraction");
        if (interactionChild == null)
        {
            Debug.LogWarning($"[ISDK] {globe.name} 에서 ISDK_RayGrabInteraction을 찾을 수 없습니다.");
            yield break;
        }

        // 1단계 : 전체 비활성화
        interactionChild.gameObject.SetActive(false);
        yield return null;
        yield return null;

        // 2단계 : 재활성화 (Awake/Start 재실행)
        interactionChild.gameObject.SetActive(true);
        yield return null;
        yield return null;

        // 3단계 : Rigidbody 확인
        Rigidbody rb = interactionChild.GetComponentInChildren<Rigidbody>(true);
        if (rb == null)
        {
            rb = interactionChild.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log($"[ISDK] {globe.name} Rigidbody 없어서 추가함");
        }

        // 4단계 : 컴포넌트 개별 토글 (의존 순서 보장)
        string[] reinitOrder = new string[]
        {
        "ColliderSurface",
        "MoveFromTargetProvider",
        "Grabbable",
        "RayInteractable"
        };

        foreach (string typeName in reinitOrder)
        {
            MonoBehaviour[] components = interactionChild.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour comp in components)
            {
                if (comp.GetType().Name.Contains(typeName))
                {
                    comp.enabled = false;
                    yield return null;
                    comp.enabled = true;
                    yield return null;
                    Debug.Log($"[ISDK] {typeName} 재초기화 완료");
                }
            }
        }

        Debug.Log($"[ISDK] {globe.name} 전체 재초기화 완료");
    }



    private void ApplyTextureMaterial(GameObject target, int index)
    {
        // Index 기반 ImageSetData 주입
        var sel = target.GetComponent<SelectableObject>();
        if (sel != null) sel.SetGlobeIndex(index);

        if (baseMaterial == null) { Debug.LogWarning("Base Material is null."); return; }

        // ★ globeTextures 대신 ImageSetData.mainImage 사용
        Texture2D texture = null;
        if (sel != null)
        {
            ImageSetData imageSet = sel.GetImageSet();
            if (imageSet != null) texture = imageSet.mainImage;
        }

        // ImageSetData에서 못 가져오면 기존 globeTextures로 fallback
        if (texture == null)
        {
            Debug.LogWarning($"[ApplyTextureMaterial] ImageSetData에서 mainImage를 가져오지 못했습니다. globeTextures로 대체합니다.");
            if (globeTextures == null || globeTextures.Length == 0) { Debug.LogWarning("Globe Textures is empty."); return; }
            texture = globeTextures[index % globeTextures.Length];
        }

        if (texture == null) { Debug.LogWarning($"Texture at index {index} is null."); return; }

        Material newMaterial = new Material(baseMaterial);
        newMaterial.enableInstancing = true;
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

    //Radius 체크용
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPosition = domeCenter != null ? domeCenter.position : transform.position;

        // 1. Visible Radius (가장 안쪽 - 녹색)
        Gizmos.color = Color.green;
        DrawWireCircle(centerPosition, visibleRadius);

        // 2. Shrink Start Radius (중간 영역 - 황색)
        Gizmos.color = Color.yellow;
        DrawWireCircle(centerPosition, shrinkStartRadius);

        // 3. Hidden Radius (가장 바깥쪽 - 적색)
        Gizmos.color = Color.red;
        DrawWireCircle(centerPosition, hiddenRadius);
    }

    private void DrawWireCircle(Vector3 center, float radius)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.up, radius);
#endif
    }
}
