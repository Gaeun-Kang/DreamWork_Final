using UnityEngine;

public class DreamGlobeScaleByCenterDistance : MonoBehaviour
{
    [Header("Center")]
    public Transform domeCenter;

    [Header("Radius Range")]
    public float visibleRadius = 1.7f;      // 이 거리 이하가 되면 나타날 준비
    public float shrinkStartRadius = 4.5f;  // 이 거리부터 줄어들기 시작
    public float hiddenRadius = 5.1f;       // 이 거리 이상이면 완전히 사라짐

    [Header("Appear Delay")]
    public float appearDelay = 1.0f;        // visibleRadius 도달 후 몇 초 뒤 나타날지

    [Header("Scale")]
    public float minScale = 0f;
    public float maxScale = 1f;
    public float shrinkSmoothSpeed = 8f;

    [Header("Debug")]
    public float currentDistance;
    public float scaleValue;
    public bool hasAppeared;
    public float appearTimer;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale * minScale;
        scaleValue = minScale;
        hasAppeared = false;
        appearTimer = 0f;
    }

    void LateUpdate()
    {
        if (domeCenter == null) return;

        currentDistance = Vector3.Distance(domeCenter.position, transform.position);

        // 1. 아직 안 나타난 상태
        if (!hasAppeared)
        {
            // visibleRadius 안에 들어오면 타이머 시작
            if (currentDistance <= visibleRadius)
            {
                appearTimer += Time.deltaTime;

                if (appearTimer >= appearDelay)
                {
                    hasAppeared = true;
                    scaleValue = maxScale;
                    transform.localScale = originalScale * maxScale;
                    return;
                }
            }
            else
            {
                // 다시 멀어지면 타이머 리셋
                appearTimer = 0f;
            }

            scaleValue = minScale;
            transform.localScale = originalScale * minScale;
            return;
        }

        // 2. 이미 나타난 뒤, shrinkStartRadius 전까지는 계속 Scale 1 유지
        if (currentDistance < shrinkStartRadius)
        {
            scaleValue = maxScale;
            transform.localScale = originalScale * maxScale;
            return;
        }

        // 3. shrinkStartRadius ~ hiddenRadius 구간에서 점점 작아짐
        if (currentDistance >= shrinkStartRadius && currentDistance < hiddenRadius)
        {
            float t = Mathf.InverseLerp(shrinkStartRadius, hiddenRadius, currentDistance);
            t = Mathf.Clamp01(t);
            t = Mathf.SmoothStep(0f, 1f, t);

            float targetScaleValue = Mathf.Lerp(maxScale, minScale, t);
            Vector3 targetScale = originalScale * targetScaleValue;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * shrinkSmoothSpeed
            );

            scaleValue = transform.localScale.x / originalScale.x;
            return;
        }

        // 4. hiddenRadius 이상이면 완전히 사라지고 다음 루프 대기
        if (currentDistance >= hiddenRadius)
        {
            hasAppeared = false;
            appearTimer = 0f;
            scaleValue = minScale;
            transform.localScale = originalScale * minScale;
        }
    }
}