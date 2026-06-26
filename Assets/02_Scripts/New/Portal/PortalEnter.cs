using UnityEngine;

public class PortalEnter : MonoBehaviour
{
    [Header("연결 컴포넌트")]
    [SerializeField] private PortallMaterialController portalMaterialController;
    [SerializeField] private LevelTransition levelTransitionManager;

    [Header("플레이어 태그 (진입 감지용)")]
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        if (portalMaterialController == null) Debug.LogError("portalMaterialController 누락");

        if (levelTransitionManager == null) Debug.LogError("LevelTransition 누락");
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        if (portalMaterialController == null)
        {
            Debug.LogWarning("[PortalTrigger] PortalMaterialController가 없습니다.");
            return;
        }

        if (levelTransitionManager == null)
        {
            Debug.LogWarning("[PortalTrigger] LevelTransitionManager가 없습니다.");
            return;
        }

        // ★ PortalMaterialController가 보관 중인 ImageSet을 가져옴
        ImageSetData currentImageSet = portalMaterialController.GetCurrentImageSet();
        if (currentImageSet == null)
        {
            Debug.LogWarning("[PortalTrigger] 선택된 ImageSet이 없습니다. Globe를 먼저 선택하세요.");
            return;
        }

        Debug.Log($"[PortalTrigger] 플레이어 포탈 진입 — Set [{currentImageSet.setID}]");
       // FadeOut.Instance.StartFadeToBlack();
        levelTransitionManager.EnterPortal(currentImageSet);
    }
}
