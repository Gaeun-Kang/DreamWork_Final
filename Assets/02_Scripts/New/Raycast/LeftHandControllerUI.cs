using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeftHandControllerUI : MonoBehaviour
{
    // 클래스 이름과 싱글톤 인스턴스 타입을 일치시켜 에러 방지
    public static LeftHandControllerUI Instance { get; private set; }

    [Header("Ray Interaction")]
    [SerializeField] private RayInteractor rayInteractor;

    [Header("UI Windows (X Button)")]
    [Tooltip("MainScene_DD03 씬에서 띄울 종료 UI")]
    [SerializeField] private GameObject exitUiObject;

    [Tooltip("New_DD05 씬에서 띄울 이전 씬 돌아가기 UI")]
    [SerializeField] private GameObject returnUiObject;

    [Header("UI Windows (Y Button)")]
    [Tooltip("Y 버튼을 눌렀을 때 띄울 재시작 확인 UI")]
    [SerializeField] private GameObject restartUiObject;

    [Header("UI Spawn Settings")]
    [Tooltip("유저 눈앞에 UI를 배치할 거리 (미터 단위)")]
    [SerializeField] private float uiSpawnDistance = 1.5f;

    private OVRInput.Controller leftController = OVRInput.Controller.LTouch;

    private void Awake()
    {
        // --- DDOL 및 싱글톤 구조화 ---
        if (Instance == null)
        {
            Instance = this;
            // 최상위 플레이어 부모 오브젝트를 찾아 DontDestroyOnLoad 처리
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            // 중복 생성 방지: 이미 인스턴스가 있다면 새로 생긴 플레이어 세트를 통째로 파괴
            Destroy(transform.root.gameObject);
            return;
        }
    }

    private void Update()
    {
        // 오직 버튼 입력(X, Y) 처리만 수행합니다.
        HandleButtonInputs();
    }

    private void HandleButtonInputs()
    {
        // --- X 버튼 처리 ---
        if (OVRInput.GetDown(OVRInput.RawButton.X, leftController))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXByIndex(4, volume: 0.2f);
            }

            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "MainScene_DD03")
            {
                ToggleAndPositionUI(exitUiObject);
            }
            else if (currentSceneName == "New_DD05")
            {
                ToggleAndPositionUI(returnUiObject);
            }
        }

        // --- Y 버튼 처리 ---
        if (OVRInput.GetDown(OVRInput.RawButton.Y, leftController))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXByIndex(4, volume: 0.2f);
            }

            ToggleAndPositionUI(restartUiObject);
        }
    }

    /// <summary>
    /// 모든 UI 창을 강제로 보이지 않게 꺼버리는 함수
    /// </summary>
    private void CloseAllUI(GameObject exception = null)
    {
        if (exitUiObject != null && exitUiObject != exception) exitUiObject.SetActive(false);
        if (returnUiObject != null && returnUiObject != exception) returnUiObject.SetActive(false);
        if (restartUiObject != null && restartUiObject != exception) restartUiObject.SetActive(false);
    }

    /// <summary>
    /// UI를 토글하며, 켜질 때 다른 UI를 다 끄고 유저 눈앞으로 정렬하는 함수
    /// </summary>
    private void ToggleAndPositionUI(GameObject targetUi)
    {
        if (targetUi == null)
        {
            Debug.LogWarning("[LeftHandControllerUI] 요청된 UI 오브젝트가 할당되지 않았습니다.");
            return;
        }

        bool nextState = !targetUi.activeSelf;

        if (nextState == true)
        {
            // 1. UI 중복 켜짐 방지
            CloseAllUI(exception: targetUi);

            // 2. UI 조준을 위해 레이를 강제로 켜줌
            SetRayActive(true);

            // 3. 플레이어 VR 카메라 정면에 UI 배치 및 정렬
            if (PlayerRigRef.Instance != null && PlayerRigRef.Instance.CenterEyeAnchor != null)
            {
                Transform camTransform = PlayerRigRef.Instance.CenterEyeAnchor;

                // 시야 정면 기준 배치
                Vector3 targetPosition = camTransform.position + (camTransform.forward * uiSpawnDistance);
                targetUi.transform.position = targetPosition;

                // 유저를 똑바로 바라보도록 회전 보정 (정면 렌더링)
                targetUi.transform.LookAt(camTransform.position);
                targetUi.transform.Rotate(0, 180f, 0);
            }
        }
        else
        {
            // UI가 꺼질 때는 레이를 같이 꺼주어 평소 화면을 깔끔하게 유지합니다.
            // 만약 분리된 이동 스크립트 쪽에서 레이 제어를 다 하도록 바꾸셨다면 이 줄은 지우셔도 됩니다.
            SetRayActive(false);
        }

        targetUi.SetActive(nextState);
    }

    /// <summary>
    /// 오큘러스 레이 인터랙터 컴포넌트 및 오브젝트를 제어하는 함수
    /// </summary>
    public void SetRayActive(bool active)
    {
        if (rayInteractor == null) return;

        // 컴포넌트 자체 활성화 비활성화
        rayInteractor.enabled = active;

        // 레이 비주얼 오브젝트 토글
        if (rayInteractor.gameObject != this.gameObject)
        {
            rayInteractor.gameObject.SetActive(active);
        }
    }

    // ==========================================
    //   UI Canvas 내 Button 연동용 Public 함수들
    // ==========================================

    public void TriggerRestart()
    {
        CloseAllUI();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void TriggerExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadPreviousScene()
    {
        CloseAllUI();
        SceneManager.LoadScene("MainScene_DD03");
    }
}