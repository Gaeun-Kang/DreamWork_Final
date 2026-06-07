using System.Collections.Generic;
using UnityEngine;

public class AvatarAppearance : MonoBehaviour
{

    [Header("대상 설정")]
    [SerializeField] private string soulTag = "Soul";

    // 정확한 거리 계산을 위한 플레이어 기준점 (비워두면 자동으로 메인 카메라를 잡습니다)
    [SerializeField] private Transform playerTargetTransform;

    [Header("거리 기준 설정 (미터 단위)")]
    [SerializeField] private float hideDistance = 0.4f; // 이보다 가까우면 투명화
    [SerializeField] private float showDistance = 0.5f; // 이보다 멀어지면 복구

    private GameObject _soulObject;
    private HashSet<Material> _soulMaterials = new HashSet<Material>();
    private static readonly int AlphaClipProp = Shader.PropertyToID("_Cutoff");

    private bool _isCurrentlyHidden = false;
    private int _lastChildCount = -1;

    void Start()
    {
        _soulObject = GameObject.FindWithTag(soulTag);
        if (_soulObject != null)
        {
            Debug.Log($"<color=cyan>[AvatarAppearance]</color> 부모 뼈대인 '{soulTag}' 오브젝트를 연결했습니다: {_soulObject.name}");
        }

        if (playerTargetTransform == null && Camera.main != null)
        {
            playerTargetTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (_soulObject == null || playerTargetTransform == null) return;

        // 1. [동적 캐싱] 하위 자식 개수가 바뀌면 실시간으로 머티리얼을 새로 긁어모음 (콜라이더 필요 없음)
        int currentChildCount = _soulObject.GetComponentsInChildren<Transform>(true).Length;
        if (currentChildCount != _lastChildCount)
        {
            _lastChildCount = currentChildCount;
            CheckAndCacheDynamicMaterials(_soulObject);

            // 새로 생성된 머티리얼이 숨김 상태 도중에 생성되었다면 즉시 투명화 적용하기 위한 조치
            if (_isCurrentlyHidden) SetAllAlphaClip(1.0f);
        }

        // 2. [거리 기반 판정] 플레이어 기준점과 Soul 중심점 사이의 실제 거리를 계산
        float currentDistance = Vector3.Distance(playerTargetTransform.position, _soulObject.transform.position);

        // 3. [상태 제어] 물리 이벤트 없이 거리에 따라 즉시 알파 전환
        if (!_isCurrentlyHidden)
        {
            if (currentDistance < hideDistance)
            {
                _isCurrentlyHidden = true;
                SetAllAlphaClip(1.0f);
                Debug.Log($"<color=red><b>[상태 변경] Soul이 몸 안에 있음 (거리: {currentDistance:F2}m)</b></color> -> 투명화");
            }
        }
        else
        {
            if (currentDistance >= showDistance)
            {
                _isCurrentlyHidden = false;
                SetAllAlphaClip(0.0f);
                Debug.Log($"<color=green><b>[상태 변경] Soul이 완전히 탈출함 (거리: {currentDistance:F2}m)</b></color> -> 불투명 복구");
            }
        }
    }

    private void CheckAndCacheDynamicMaterials(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.materials;
            foreach (Material mat in mats)
            {
                if (!_soulMaterials.Contains(mat) && mat.HasProperty(AlphaClipProp))
                {
                    _soulMaterials.Add(mat);
                    Debug.Log($"<color=teal>[동적 캐싱]</color> 새로운 자식 머티리얼 실시간 발견: {rend.name} -> {mat.name}");
                }
            }
        }
    }

    private void SetAllAlphaClip(float value)
    {
        foreach (Material mat in _soulMaterials)
        {
            if (mat != null) mat.SetFloat(AlphaClipProp, value);
        }
    }

    private void OnDestroy()
    {
        foreach (Material mat in _soulMaterials)
        {
            if (mat != null) Destroy(mat);
        }
        _soulMaterials.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        // 기준점이 명시되어 있다면 그 위치를 중심으로 그리고, 비어있다면 현재 컴포넌트 위치를 기준으로 삼습니다.
        Vector3 centerPosition = (playerTargetTransform != null) ? playerTargetTransform.position : transform.position;

        // 1. Hide Distance (진입 시 사라지는 영역) : 빨간색 반투명 구체
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.2f); // 내부를 채울 반투명한 색상 (알파 0.2)
        Gizmos.DrawSphere(centerPosition, hideDistance);
        Gizmos.color = Color.red; // 외곽선 선명하게
        Gizmos.DrawWireSphere(centerPosition, hideDistance);

        // 2. Show Distance (탈출 시 나타나는 영역) : 초록색 반투명 구체
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.1f); // 조금 더 넓은 탈출 반경 (알파 0.1)
        Gizmos.DrawSphere(centerPosition, showDistance);
        Gizmos.color = Color.green; // 외곽선 선명하게
        Gizmos.DrawWireSphere(centerPosition, showDistance);
    }
}
