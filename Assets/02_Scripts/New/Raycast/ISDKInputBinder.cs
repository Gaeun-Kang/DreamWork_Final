using System.Collections;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class ISDKInputBinder : MonoBehaviour
{
    void Start()
    {
        // 씬이 로드되자마자 에러 없는 안전한 재부팅 시퀀스 시작
        StartCoroutine(SafeRebootSequenceRoutine());
    }

    private IEnumerator SafeRebootSequenceRoutine()
    {
        // 1. 다른 컴포넌트들이 완전히 무대에 배치될 때까지 0.15초 대기
        yield return new WaitForSeconds(0.15f);

        // 현재 오브젝트에 붙은 RayInteractor를 가져옵니다.
        RayInteractor rayInteractor = GetComponent<RayInteractor>();
        if (rayInteractor == null)
        {
            Debug.LogError("[ISDK Binder] 이 스크립트는 RayInteractor 컴포넌트와 함께 있어야 합니다.");
            yield break;
        }

        // 2. [전체 다운] 유령 참조를 들고 있을 확률이 높은 모든 인터랙션 축을 강제로 잠재웁니다.
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = false;
        }

        rayInteractor.gameObject.SetActive(false);

        // 씬 내의 상위 컨트롤러 데이터 소스(ControllerRef)도 찾아 잠시 꺼줍니다.
        // (새 씬의 깨끗한 하드웨어 데이터를 강제로 동기화시키기 위함)
        ControllerRef controllerRef = FindObjectOfType<ControllerRef>();
        if (controllerRef != null)
        {
            controllerRef.gameObject.SetActive(false);
        }

        // 3. 유니티가 레지스트리 메모리에서 구형 유령 참조들을 완벽히 언레지스터 하도록 2프레임 대기
        yield return null;
        yield return null;

        // 4. [순서대로 업] 하드웨어 공급처(ControllerRef)를 먼저 깨웁니다.
        if (controllerRef != null)
        {
            controllerRef.gameObject.SetActive(true);
            // ControllerRef가 하드웨어와 악수할 시간을 1프레임 쉼
            yield return null;
        }

        // 5. 유니티 이벤트 시스템을 복구합니다.
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.enabled = true;
        }

        // 6. 마지막으로 레이 인터랙터를 켭니다.
        // 이제 레이가 켜지면서 방금 깨끗하게 리프레시된 새 ControllerRef의 버튼 입력 버퍼를 정상 수신합니다.
        rayInteractor.gameObject.SetActive(true);

        Debug.Log("[ISDK Binder] API 의존성 없이 안전하게 하드웨어 인풋 축 재부팅을 완료했습니다!");
    }
}