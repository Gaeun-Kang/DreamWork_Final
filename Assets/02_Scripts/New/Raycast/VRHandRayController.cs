using System.Collections;
using UnityEngine;

public class VRHandRayController : MonoBehaviour
{
    // 외부에서 언제든 접근할 수 있도록 싱글톤 지정 (해당 씬에서만 유지되므로 안전)
    public static VRHandRayController Instance { get; private set; }

    [Header("Oculus ISDK Hand")]
    [SerializeField] private GameObject m_HandRayInteractorObj;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 씬 시작하자마자 기본적으로 레이가 작동할 수 있도록 보장
        if (m_HandRayInteractorObj != null)
        {
            m_HandRayInteractorObj.SetActive(true);
        }
    }

    /// <summary>
    /// 동적 오브젝트들이 생성을 마친 후 이 함수를 호출해달라고 요청할 것입니다.
    /// </summary>
    public void RequestRayReboot()
    {
        StartCoroutine(RayRebootRoutine());
    }

    private IEnumerator RayRebootRoutine()
    {
        if (m_HandRayInteractorObj == null) yield break;

        // 1. 유니티 UI 이벤트 캐시 정리를 위해 잠시 끔
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.enabled = false;

        // 2. 레이 인터랙터 비활성화 (레지스트리에서 유령 참조 완전 분리)
        m_HandRayInteractorObj.SetActive(false);

        // 3. 확실하게 언레지스터가 반영되도록 2프레임 대기
        yield return null;
        yield return null;

        // 4. 유니티 UI 이벤트 시스템 복구
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.enabled = true;

        // 5. 레이 다시 켜기 -> 이제 이미 배치 완료된 동적 Globe들을 깨끗하게 인식함
        m_HandRayInteractorObj.SetActive(true);
        
        Debug.Log("[ISDK] 동적 오브젝트 배치 감지 후, HandRayInteractor 재부팅 완료!");
    }
}