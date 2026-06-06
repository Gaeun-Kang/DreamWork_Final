using UnityEngine;

public class DD05SceneInitializer : MonoBehaviour
{
    [Header("DD05 Skybox 컨트롤러")]
    [SerializeField] private DD05SkyboxController skyboxController;


    private void Awake()
    {
        if (skyboxController == null)
        {

            Debug.LogError("[DD05SceneInitializer] DD05SkyboxController를 씬에서 찾을 수 없습니다!");
            return;

        }

        // 포탈에서 전달된 pending ImageSet을 Skybox에 적용
        LevelTransition.ApplyPendingImageSet(skyboxController);

    }
}
