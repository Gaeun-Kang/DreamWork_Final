
using System.Collections;
using UnityEngine;

/// DD05 레벨의 Skybox Material 텍스쳐를 동적으로 변경.
/// Scene에 단 하나만 존재해야 하며, LevelTransitionManager가 진입 시 호출.

public class DD05SkyboxController : MonoBehaviour
{
    [Header("Skybox Material (Procedural 혹은 Cubemap/Panoramic)")]
    [SerializeField] private Material skyboxMaterial;

    [Header("텍스쳐 프로퍼티 이름 (Panoramic: _MainTex / Cubemap: _Tex)")]
    [SerializeField] private string skyboxTextureProperty = "_Tex";

    [Header("Tint 색상 프로퍼티 (선택)")]
    [SerializeField] private string tintProperty = "_Tint";
    [SerializeField] private Color defaultTint = Color.white;

    [Header("전환 설정")]
    [SerializeField] private bool useTransition = true;
    [SerializeField] private float transitionDuration = 1.0f;

    public ImageSetData CurrentImageSet { get; private set; }
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("[DD05SkyboxController] Skybox Material이 할당되지 않았습니다!");
            return;
        }

        skyboxMaterial = new Material(skyboxMaterial);

        // Scene의 Skybox를 해당 Material로 설정
        RenderSettings.skybox = skyboxMaterial;

        //시작은 검은 화면 
        if (skyboxMaterial.HasProperty(tintProperty))
        {
            skyboxMaterial.SetColor(tintProperty, Color.black);
        }

    }

    /// LevelTransitionManager: Real Image를 Skybox Texture로 적용

    public void ApplyRealImageAsSkybox(ImageSetData imageSet)
    {

        if (imageSet == null)
        {
            Debug.LogWarning("[DD05SkyboxController] ImageSetData가 null입니다.");
            return;
        }



        if (imageSet.realImage == null)
        {
            Debug.LogWarning($"[DD05SkyboxController] Set [{imageSet.setID}]의 Real Image(Cubemap)가 없습니다.");
            return;
        }
        CurrentImageSet = imageSet;


        if (useTransition && gameObject.activeInHierarchy)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionSkybox(imageSet.realImage, imageSet.setID));
        }
        else
        {
            SetSkyboxCubemap(imageSet.realImage);
            Debug.Log($"[DD05SkyboxController] Skybox → Cubemap [{imageSet.setID}] 즉시 적용");
        }

    }

    /// Cubemap을 Skybox Material에 즉시 적용하고 GI를 갱신합니다.
    private void SetSkyboxCubemap(Cubemap cubemap)
    {
        skyboxMaterial.SetTexture(skyboxTextureProperty, cubemap);
        DynamicGI.UpdateEnvironment();
    }

    private IEnumerator TransitionSkybox(Cubemap newCubemap, string setID)
    {
        bool hasTint = skyboxMaterial.HasProperty(tintProperty);
        float half = transitionDuration * 0.5f;
        float elapsed = 0f;

        //큐브맵 교체 

        SetSkyboxCubemap(newCubemap);
        Debug.Log($"[DD05SkyboxController] Skybox → Cubemap [{setID}] 전환 완료");

        //Fade In
        if (hasTint)
        {

            elapsed = 0f;

            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                skyboxMaterial.SetColor(tintProperty, Color.Lerp(Color.black, defaultTint, t));
                yield return null;
            }
            skyboxMaterial.SetColor(tintProperty, defaultTint);
        }

        _transitionCoroutine = null;
    }
    public void CancelTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
        if (skyboxMaterial != null && skyboxMaterial.HasProperty(tintProperty))
            skyboxMaterial.SetColor(tintProperty, defaultTint);
    }

}
