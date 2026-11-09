using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{

    private static ImageSetData _StaticImageSet;

    [SerializeField] private string NextSceneName;

    [Header("DD05 씬 이름")]
    [SerializeField] private string dd05SceneName = "DD05";

    [Header("전환 전 딜레이 (초)")]
    [SerializeField] private float transitionDelay = 0.5f;

    /// 포탈 진입 시 외부에서 호출 (VR grabble 등)
    /// 
    public static LevelTransition Instance;

    private void Awake()
    {
      if(Instance == null)
        { Instance = this; }
        else 
        { Destroy(gameObject); }
    }
    public void EnterPortal(ImageSetData selectedImageSet)
    {
        if (selectedImageSet == null) return;
        SoundManager.Instance.PlaySFXByIndex(5, volume: 0.4f);
        //씬전환동안 RayInteractor 비활성화 
        var rayInteractors = Object.FindObjectsByType<RayInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in rayInteractors)
        {
            interactor.gameObject.SetActive(false);
        }


        _StaticImageSet = selectedImageSet;
        StartCoroutine(LoadDD05(selectedImageSet));
    }

    private IEnumerator LoadDD05(ImageSetData imageSet)
    {
        yield return new WaitForSeconds(transitionDelay);

        // 씬 전환 직전, 활성 상태의 모든 RayInteractable을 강제로 Disable
        // (비활성 상태였던 것도 깨워서 OnDisable 트리거)
        var allInteractables = Object.FindObjectsByType<RayInteractable>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ri in allInteractables)
        {
            if (!ri.gameObject.activeInHierarchy)
            {
                ri.gameObject.SetActive(true); // 강제 활성화
            }
            ri.enabled = false; // OnDisable 명시적 트리거 → Unregister 보장
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(dd05SceneName, LoadSceneMode.Single);
        while (!op.isDone)
        {
            yield return null;
        }
        yield return null;
    }

    private void ApplySkyboxInDD05()
    {
        if (_StaticImageSet == null) return;

        var skyboxController = Object.FindFirstObjectByType<DD05SkyboxController>();
        if (skyboxController != null)
        {
            skyboxController.ApplyRealImageAsSkybox(_StaticImageSet);
        }
        else
        {
            Debug.LogError("[LevelTransitionManager] DD05SkyboxController를 찾을 수 없습니다!");
        }

        _StaticImageSet = null;
    }

    /// DD05 씬에서 직접 호출 가능 (씬 시작 시 이미지 데이터 적용)
    public static void ApplyPendingImageSet(DD05SkyboxController skyboxController)
    {
        if (_StaticImageSet != null && skyboxController != null)
        {
            skyboxController.ApplyRealImageAsSkybox(_StaticImageSet);
            _StaticImageSet = null;
        }
    }
    public static ImageSetData GetPendingImageSet()
    {
        return _StaticImageSet;
    }

    public static void ClearPendingImageSet()
    {
        _StaticImageSet = null;
    }

}
