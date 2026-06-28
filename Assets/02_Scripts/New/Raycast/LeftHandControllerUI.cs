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

    /*
    [Header("Canvas Setting")]
    [Tooltip("상호작용할 최상위 부모 Canvas 오브젝트 (평소엔 꺼두고 UI 팝업 시 켜짐)")]
    [SerializeField] private GameObject controllerUiCanvas;

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
    */

    private OVRInput.Controller leftController = OVRInput.Controller.LTouch;
/*
    private void Awake()
    {
        if (controllerUiCanvas == null)
        {

            controllerUiCanvas = GameObject.Find("Controller_UI");
        }
    }
*/
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
                TriggerExitGame();
                //ToggleAndPositionUI(exitUiObject);
            }
            else if (currentSceneName == "New_DD05")
            {
                LoadPreviousScene();
                //ToggleAndPositionUI(returnUiObject);
            }
        }

        // --- Y 버튼 처리 ---
        if (OVRInput.GetDown(OVRInput.RawButton.Y, leftController))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXByIndex(4, volume: 0.2f);
              
            }

            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "MainScene_DD03")
            {
                TriggerRestart();
                //ToggleAndPositionUI(exitUiObject);
            }
            else if (currentSceneName == "New_DD05")
            {
                TriggerExitGame();
                //ToggleAndPositionUI(returnUiObject);
            }

            //  ToggleAndPositionUI(restartUiObject);
        }
    }

    /*
       private void CloseAllUI(GameObject exception = null)
       {
           if (exitUiObject != null && exitUiObject != exception) exitUiObject.SetActive(false);
           if (returnUiObject != null && returnUiObject != exception) returnUiObject.SetActive(false);
           if (restartUiObject != null && restartUiObject != exception) restartUiObject.SetActive(false);
       }


       private void ToggleAndPositionUI(GameObject targetUi)
       {
           if (targetUi == null)
           {
               Debug.LogWarning("[LeftHandControllerUI] 요청된 UI 오브젝트가 할당되지 않았습니다.");
               return;
           }

           // targetUi가 현재 켜져있는지 여부를 판단
           bool isUiCurrentlyActive = targetUi.activeSelf;

           // 만약 메인 캔버스 자체가 꺼져있었다면, 무조건 UI가 꺼져있던 것으로 간주합니다.
           if (controllerUiCanvas != null && !controllerUiCanvas.activeSelf)
           {
               isUiCurrentlyActive = false;
           }

           bool nextState = !isUiCurrentlyActive;

           if (nextState == true)
           {
               // 1. 하위 UI 중복 켜짐 방지
               CloseAllUI(exception: targetUi);
               targetUi.SetActive(true);

               // 2. 평소에 꺼두었던 메인 캔버스 오브젝트를 먼저 활성화합니다.
               if (controllerUiCanvas != null)
               {
                   controllerUiCanvas.SetActive(true);
               }

               // 3. UI 조준을 위해 레이를 강제로 켜줌
               SetRayActive(true);

               // 4. 최상위 부모(Canvas)를 플레이어 VR 카메라 정면에 배치 및 정렬
               // 자식 UI들이 움직이는 것이 아니라 캔버스 전체가 움직여야 인터랙션(OVRRaycaster 등)이 깨지지 않습니다.
               GameObject objectToMove = controllerUiCanvas != null ? controllerUiCanvas : gameObject;

               if (PlayerRigRef.Instance != null && PlayerRigRef.Instance.CenterEyeAnchor != null)
               {
                   Transform camTransform = PlayerRigRef.Instance.CenterEyeAnchor;

                   // 시야 정면 기준 배치
                   Vector3 targetPosition = camTransform.position + (camTransform.forward * uiSpawnDistance);
                   objectToMove.transform.position = targetPosition;

                   // 유저를 똑바로 바라보도록 회전 보정 (정면 렌더링)
                   objectToMove.transform.LookAt(camTransform.position);
                   objectToMove.transform.Rotate(0, 180f, 0);
               }
           }
           else
           {
               // UI를 끌 때는 개별 UI 창을 끄고, 메인 캔버스 전체도 함께 비활성화합니다.
               targetUi.SetActive(false);

               if (controllerUiCanvas != null)
               {
                   controllerUiCanvas.SetActive(false);
               }

               SetRayActive(false);
           }

}
           */

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

    public void TriggerRestart()
    {
       // CloseAllUI();
       // if (controllerUiCanvas != null) controllerUiCanvas.SetActive(false);

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
      //  CloseAllUI();
       // if (controllerUiCanvas != null) controllerUiCanvas.SetActive(false);

        SceneManager.LoadScene("MainScene_DD03");
    }
}