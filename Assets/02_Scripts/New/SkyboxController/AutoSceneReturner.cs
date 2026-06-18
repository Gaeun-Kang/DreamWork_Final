using System.Collections;
using UnityEngine;

public class AutoSceneReturner : MonoBehaviour
{
    [Header("시간 설정")]
    [Tooltip("이전 씬으로 돌아가기 전 대기 시간 (초 단위)")]
    [SerializeField] private float waitDuration = 15f;

    void Start()
    {
        // 씬이 시작되자마자 타이머 코루틴 시작
        StartCoroutine(AutoReturnRoutine());
    }

    private IEnumerator AutoReturnRoutine()
    {
        // 1. 지정된 시간(15초) 동안 대기
        yield return new WaitForSeconds(waitDuration);

        // 2. 대기 시간이 끝나면 바로 이전 씬으로 돌아갑니다.
        if (LeftHandControllerUI.Instance != null)
        {
            // LeftHandControllerUI에 작성해둔 UI 끄기 및 MainScene_DD03 로드 함수 실행
            LeftHandControllerUI.Instance.LoadPreviousScene();
        }
        else
        {
            Debug.LogError("[AutoSceneReturner] LeftHandControllerUI 인스턴스를 찾을 수 없습니다.");
        }
    }
}