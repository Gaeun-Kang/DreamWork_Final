
using UnityEngine;

/// DD05 레벨의 Skybox Material 텍스쳐를 동적으로 변경.
/// Scene에 단 하나만 존재해야 하며, LevelTransitionManager가 진입 시 호출.

public class DD05SkyboxController : MonoBehaviour
{
    [Header("Skybox Material (Procedural 혹은 Cubemap/Panoramic)")]
    [SerializeField] private Material skyboxMaterial;

    [Header("텍스쳐 프로퍼티 이름 (Panoramic: _MainTex / Cubemap: _Tex)")]
    [SerializeField] private string skyboxTextureProperty = "_MainTex";

    [Header("Tint 색상 프로퍼티 (선택)")]
    [SerializeField] private string tintProperty = "_Tint";
    [SerializeField] private Color defaultTint = Color.white;

    [Header("전환 설정")]
    [SerializeField] private bool useTransition = true;
    [SerializeField] private float transitionDuration = 1.0f;

    private Coroutine _transitionCoroutine;
    private Texture2D _currentTexture;

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
            Debug.LogWarning($"[DD05SkyboxController] Set [{imageSet.setID}]의 Real Image가 없습니다.");
            return;
        }

        if (useTransition)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionSkybox(imageSet.realImage));
        }
        else
        {
            SetSkyboxTexture(imageSet.realImage);
        }

        Debug.Log($"[DD05SkyboxController] Skybox → Real Image [{imageSet.setID}] 적용 완료");
    }

    private void SetSkyboxTexture(Texture2D texture)
    {
        _currentTexture = texture;
        skyboxMaterial.SetTexture(skyboxTextureProperty, texture);
        DynamicGI.UpdateEnvironment(); // GI 갱신
    }

    private System.Collections.IEnumerator TransitionSkybox(Texture2D newTexture)
    {
        float elapsed = 0f;
        float half = transitionDuration * 0.5f;

        // Fade out (Tint → Black)
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            if (skyboxMaterial.HasProperty(tintProperty))
                skyboxMaterial.SetColor(tintProperty, Color.Lerp(defaultTint, Color.black, t));
            yield return null;
        }

        // 텍스쳐 교체
        SetSkyboxTexture(newTexture);
        Debug.Log($"[DD05SkyboxController] 코루틴 내부: 텍스처 교체 완료");

        // Fade in (Black → defaultTint)
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            if (skyboxMaterial.HasProperty(tintProperty))
                skyboxMaterial.SetColor(tintProperty, Color.Lerp(Color.black, defaultTint, t));
            yield return null;
        }

        if (skyboxMaterial.HasProperty(tintProperty))
            skyboxMaterial.SetColor(tintProperty, defaultTint);
    }
}
