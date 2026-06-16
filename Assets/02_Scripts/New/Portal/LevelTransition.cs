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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void EnterPortal(ImageSetData selectedImageSet)
    {
        if (selectedImageSet == null)
        {
            Debug.LogWarning("[LevelTransitionManager] 선택된 ImageSet이 없습니다.");
            return;
        }

        _StaticImageSet = selectedImageSet;
        Debug.Log($"[LevelTransitionManager] 포탈 진입 → DD05, Set [{selectedImageSet.setID}]");
        StartCoroutine(LoadDD05(selectedImageSet));
    }

    private IEnumerator LoadDD05(ImageSetData imageSet)
    {
        yield return new WaitForSeconds(transitionDelay);

        SoundManager.Instance.PlayBGM(SoundManager.GameEvent.World_Trans);

        // Additive 방식이 아닌 Single 씬 전환
        AsyncOperation op = SceneManager.LoadSceneAsync(dd05SceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        // 씬 로드 완료 후 Skybox 적용
        yield return null; // 한 프레임 대기 (DD05SkyboxController Awake 완료 보장)
        ApplySkyboxInDD05();
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


}
