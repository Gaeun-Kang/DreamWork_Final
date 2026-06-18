using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeftHandControllerUI : MonoBehaviour 
{
    public static SimpleLocomotionController Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 90f;   // degrees per second

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

    private float moveInput;
    private bool isMoving = false;
    private OVRInput.Controller leftController = OVRInput.Controller.LTouch;

    private void Awake()
    {
        // --- DontDestroyOnLoad 및 싱글톤 세팅 ---
        if (Instance == null)
        {
        
            // 이 오브젝트와 자식 오브젝트(UI 포함)들을 다른 씬에서도 유지
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 다른 씬에서 중복으로 생성된 매니저가 있다면 즉시 파괴
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // 1. 이동 input 처리
        moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, leftController).y;
        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(moveInput) > 0.1f;

        // 상태 변화 시에만 레이 활성화/비활성화 토글
        if (wasMoving != isMoving)
        {
            SetRayActive(!isMoving);
        }

        // 2. 버튼 input 처리
        HandleButtonInputs();
    }

    private void HandleButtonInputs()
    {
   
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
        // 매개변수로 들어온 exception(현재 켜려는 UI)을 제외한 나머지를 전부 비활성화
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
            Debug.LogWarning("[Movement] 요청된 UI 오브젝트가 할당되지 않았습니다.");
            return;
        }

        // 다음으로 변경할 상태값 계산
        bool nextState = !targetUi.activeSelf;

        if (nextState == true)
        {
            // 1. UI를 새로 키는 상황이라면 다른 모든 UI를 먼저 꺼버림 (중복 방지 핵심)
            CloseAllUI(exception: targetUi);

            // 2. 켜지는 순간 유저의 현재 시야 앞으로 텔레포트 및 정렬
            if (PlayerRigRef.Instance != null && PlayerRigRef.Instance.CenterEyeAnchor != null)
            {
                Transform camTransform = PlayerRigRef.Instance.CenterEyeAnchor;

                // 시야 정면 거리 계산
                Vector3 targetPosition = camTransform.position + (camTransform.forward * uiSpawnDistance);
                targetUi.transform.position = targetPosition;

                // 유저를 똑바로 바라보도록 회전 보정
                targetUi.transform.LookAt(camTransform.position);
                targetUi.transform.Rotate(0, 180f, 0);


            }
        }

        // 3. 계산된 최종 상태 적용 (켜거나 끄기)
        targetUi.SetActive(nextState);
    }

    private void SetRayActive(bool active)
    {
        if (rayInteractor == null) return;
        rayInteractor.gameObject.SetActive(active);
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        if (PlayerRigRef.Instance == null || PlayerRigRef.Instance.CenterEyeAnchor == null) return;

        Vector3 forward = new Vector3(
            PlayerRigRef.Instance.CenterEyeAnchor.forward.x, 0,
            PlayerRigRef.Instance.CenterEyeAnchor.forward.z
        ).normalized;

        Vector3 move = forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }


    public void TriggerRestart()
    {
        CloseAllUI(); // 재시작 전 UI 정리
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
        CloseAllUI(); // 씬 이동 전 UI 정리
        SceneManager.LoadScene("MainScene_DD03");
    }

}
