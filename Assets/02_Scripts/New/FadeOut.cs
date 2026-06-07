using System.Collections;
using UnityEngine;

public class FadeOut : MonoBehaviour 
{
    public static FadeOut Instance{ get; private set; }
    public float fadeDuration = 1.0f;
    private Color initialTint;

    void Start()
    {
        if (RenderSettings.skybox.HasProperty("_Tint"))
        {
            initialTint = RenderSettings.skybox.GetColor("_Tint");
        }
    }

    public void StartFadeToBlack()
    {
        StartCoroutine(FadeToBlackRoutine());
    }

    private IEnumerator FadeToBlackRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color newColor = Color.Lerp(initialTint, Color.black, t);
            yield return null;
        }

        // 정확한 검은색이 되도록 최종 설정
        RenderSettings.skybox.SetColor("_Tint", Color.black);
    }
}
