using System.Collections;
using UnityEngine;

public class FadeOut : MonoBehaviour 
{
    public static FadeOut Instance { get; private set; }

    public float fadeDuration = 15f;
    private Color initialTint;
    private Material runtimeSkybox; // 원본 손상을 막기 위한 인스턴스 런타임 머티리얼
    private bool isFading = false;  // 중복 실행 방지 플래그

    void Awake()
    {
        // 3. 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. 원본 머티리얼 복제본을 만들어 적용 (프로젝트 원본 에셋 보호)
        if (RenderSettings.skybox != null)
        {
            runtimeSkybox = new Material(RenderSettings.skybox);
            RenderSettings.skybox = runtimeSkybox;
        }
    }

    void Start()
    {
        if (runtimeSkybox != null && runtimeSkybox.HasProperty("_Tint"))
        {
            initialTint = runtimeSkybox.GetColor("_Tint");
        }
        else
        {
            // 만약 스카이박스 셰이더 속성명이 _Tint가 아닐 경우를 대비한 기본값
            initialTint = Color.white;
        }
    }

    public void StartFadeToBlack()
    {
        // 4. 이미 페이드 중이면 중복 실행하지 않음
        if (isFading) return;

        StartCoroutine(FadeToBlackRoutine());
    }

    private IEnumerator FadeToBlackRoutine()
    {
        isFading = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(initialTint, Color.black, t);

            // 1. 매 프레임 실시간으로 스카이박스 색상을 변경해줍니다.
            if (runtimeSkybox != null && runtimeSkybox.HasProperty("_Tint"))
            {
                runtimeSkybox.SetColor("_Tint", newColor);
            }

            yield return null;
        }

        // 정확한 검은색이 되도록 최종 설정
        if (runtimeSkybox != null && runtimeSkybox.HasProperty("_Tint"))
        {
            runtimeSkybox.SetColor("_Tint", Color.black);
        }

        // 씬 이동 전 플래그 해제
        isFading = false;

        // 이전 씬 로드 진행
        if (LeftHandControllerUI.Instance != null)
        {
            LeftHandControllerUI.Instance.LoadPreviousScene();
        }
        else
        {
            Debug.LogError("LeftHandControllerUI 인스턴스를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        // 씬이 완전히 바뀔 때 런타임에 생성했던 머티리얼을 메모리에서 해제해줍니다.
        if (runtimeSkybox != null)
        {
            Destroy(runtimeSkybox);
        }
    }
}
