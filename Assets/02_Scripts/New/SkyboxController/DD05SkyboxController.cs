using System.Collections;
using UnityEngine;

public class DD05SkyboxController : MonoBehaviour
{
    [Header("Skybox Material (Procedural 혹은 Cubemap/Panoramic)")]
    [SerializeField] private Material skyboxMaterial;

    [Header("텍스쳐 프로퍼티 이름 (Panoramic: _MainTex / Cubemap: _Tex)")]
    [SerializeField] private string skyboxTextureProperty = "_Tex";

    [Header("Tint 색상 프로퍼티 (선택)")]
    [SerializeField] private string tintProperty = "_Tint";
    [SerializeField] private Color defaultTint = Color.white;

    [Header("전환 설정")]
    [SerializeField] private bool useTransition = true;
    [SerializeField] private float transitionDuration = 1.0f;

    public ImageSetData CurrentImageSet { get; private set; }
    private Coroutine _transitionCoroutine;
    private Material _runtimeMaterialInstance; // 메모리 관리를 위한 인스턴스 보관 변수

    private void Awake()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("[DD05SkyboxController] Skybox Material이 할당되지 않았습니다!");
            return;
        }

        // 메모리 누수 방지를 위해 별도 변수에 생성 및 관리
        _runtimeMaterialInstance = new Material(skyboxMaterial);
    }

    private void Start()
    {
        if (_runtimeMaterialInstance == null) return;

        // 렌더링 파이프라인의 안정적 할당을 위해 Start 시점에 할당
        RenderSettings.skybox = _runtimeMaterialInstance;

        // 초기 화면을 암전 상태로 시작
        if (_runtimeMaterialInstance.HasProperty(tintProperty))
        {
            _runtimeMaterialInstance.SetColor(tintProperty, Color.black);
        }

        // 대기 중인 데이터 로드
        ImageSetData pendingData = LevelTransition.GetPendingImageSet();

        if (pendingData != null)
        {
            ApplyRealImageAsSkybox(pendingData);
            LevelTransition.ClearPendingImageSet();
        }
        else
        {
            Debug.LogWarning("[DD05SkyboxController] 적용할 대기 중인 ImageSet 데이터가 없습니다.");
            // 대기 데이터가 없다면 즉시 기본 라이팅 켜기
            if (_runtimeMaterialInstance.HasProperty(tintProperty))
                _runtimeMaterialInstance.SetColor(tintProperty, defaultTint);
        }
    }

    public void ApplyRealImageAsSkybox(ImageSetData imageSet)
    {
        if (imageSet == null || imageSet.realImage == null)
        {
            Debug.LogWarning("[DD05SkyboxController] 유효하지 않은 ImageSetData입니다.");
            return;
        }

        CurrentImageSet = imageSet;

        if (useTransition && gameObject.activeInHierarchy)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(TransitionSkybox(imageSet.realImage, imageSet.setID));
        }
        else
        {
            SetSkyboxCubemap(imageSet.realImage);
            if (_runtimeMaterialInstance.HasProperty(tintProperty))
                _runtimeMaterialInstance.SetColor(tintProperty, defaultTint);

            Debug.Log($"[DD05SkyboxController] Skybox → Cubemap [{imageSet.setID}] 즉시 적용");
        }
    }

    private void SetSkyboxCubemap(Cubemap cubemap)
    {
        if (_runtimeMaterialInstance == null) return;

        _runtimeMaterialInstance.SetTexture(skyboxTextureProperty, cubemap);

        // VR 최적화: 씬 전환 시점에 단 한 번만 호출되도록 격리 유전
        DynamicGI.UpdateEnvironment();
    }

    private IEnumerator TransitionSkybox(Cubemap newCubemap, string setID)
    {
        bool hasTint = _runtimeMaterialInstance.HasProperty(tintProperty);
        float halfDuration = transitionDuration * 0.5f;
        float elapsed = 0f;

        // [선택 사항 보완]: 만약 이미 화면이 켜진 상태에서 호출되는 경우라면, 
        // 여기서 먼저 Black으로 Fade-Out 시키는 로직을 추가하는 것이 시각적으로 안전합니다.

        // 1. 큐브맵 교체 (화면이 Black인 상태에서 교체되므로 튀는 현상 없음)
        SetSkyboxCubemap(newCubemap);
        Debug.Log($"[DD05SkyboxController] Skybox → Cubemap [{setID}] 교체 완료 및 페이드인 시작");

        // 2. Fade In (Black -> DefaultTint)
        if (hasTint)
        {
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                _runtimeMaterialInstance.SetColor(tintProperty, Color.Lerp(Color.black, defaultTint, t));
                yield return null;
            }
            _runtimeMaterialInstance.SetColor(tintProperty, defaultTint);
        }

        _transitionCoroutine = null;
    }

    public void CancelTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
        if (_runtimeMaterialInstance != null && _runtimeMaterialInstance.HasProperty(tintProperty))
            _runtimeMaterialInstance.SetColor(tintProperty, defaultTint);
    }

    // ★ [중요] 가비지 컬렉터가 수집하지 못하는 C++ 영역의 마테리얼 메모리 해제
    private void OnDestroy()
    {
        if (_runtimeMaterialInstance != null)
        {
            Destroy(_runtimeMaterialInstance);
            _runtimeMaterialInstance = null;
        }
    }
}