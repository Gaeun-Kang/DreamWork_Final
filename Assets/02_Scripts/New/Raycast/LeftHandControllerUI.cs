using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeftHandControllerUI : MonoBehaviour 
{
    [Header("UI Windows (X Button)")]
    [Tooltip("MainScene_DD03 씬에서 띄울 종료 UI")]
    [SerializeField] private GameObject exitUiObject;

    [Tooltip("New_DD05 씬에서 띄울 이전 씬 돌아가기 UI")]
    [SerializeField] private GameObject returnUiObject;

    [Header("UI Windows (Y Button)")]
    [Tooltip("Y 버튼을 눌렀을 때 띄울 재시작 확인 UI")]
    [SerializeField] private GameObject restartUiObject;

    private OVRInput.Controller leftController = OVRInput.Controller.LTouch;

    private void Update()
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
                if (exitUiObject != null)
                {
                    // UI가 꺼져있으면 켜고, 켜져있으면 끄는 토글 처리
                    bool isCurrentlyActive = exitUiObject.activeSelf;
                    exitUiObject.SetActive(!isCurrentlyActive);
                    Debug.Log($"[Movement] Exit UI 상태 변경 -> {!isCurrentlyActive}");
                }
                else
                {
                    Debug.LogWarning("[Movement] exitUiObject가 인스펙터에 할당되지 않았습니다.");
                }
            }
            else if (currentSceneName == "New_DD05")
            {
                if (returnUiObject != null)
                {
                    // UI 토글 처리
                    bool isCurrentlyActive = returnUiObject.activeSelf;
                    returnUiObject.SetActive(!isCurrentlyActive);
                    Debug.Log($"[Movement] 이전 씬 돌아가기 UI 상태 변경 -> {!isCurrentlyActive}");
                }
                else
                {
                    Debug.LogWarning("[Movement] returnUiObject가 인스펙터에 할당되지 않았습니다.");
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y, leftController))
        {

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXByIndex(4, volume: 0.2f);
            }

            if (restartUiObject != null)
            {
                // Restart UI 켜고 끄기 토글
                bool isCurrentlyActive = restartUiObject.activeSelf;
                restartUiObject.SetActive(!isCurrentlyActive);
                Debug.Log($"[Movement] Restart UI 상태 변경 -> {!isCurrentlyActive}");
            }
            else
            {
                Debug.LogWarning("[Movement] restartUiObject가 인스펙터에 할당되지 않았습니다.");
            }
        }

    }

    public void TriggerRestart()
    {
        Debug.Log("[Movement] UI를 통해 최종 씬 재시작이 승인되었습니다.");
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }


}
