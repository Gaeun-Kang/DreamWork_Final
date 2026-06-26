using UnityEngine;
using System.Collections;

public class EgoSphereFloatMotion : MonoBehaviour
{
    [Header("Rise")]
    public float startY = -1f;
    public float targetY = 1.3f;
    public float riseDuration = 2.5f;
    public bool playOnStart = true;

    [Header("Hover")]
    public float hoverAmplitude = 0.05f;   // 살짝만
    public float hoverFrequency = 1.2f;    // 동동거리는 속도

    private Vector3 baseLocalPos;
    private bool isHovering = false;
    private Coroutine moveRoutine;

    void Start()
    {
        baseLocalPos = transform.localPosition;
        baseLocalPos.y = startY;
        transform.localPosition = baseLocalPos;

        if (playOnStart)
            PlayRise();
    }

    public void PlayRise()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(RiseRoutine());
    }

    IEnumerator RiseRoutine()
    {
        isHovering = false;

        Vector3 from = transform.localPosition;
        from.y = startY;
        transform.localPosition = from;

        Vector3 to = transform.localPosition;
        to.y = targetY;

        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);

            // 부드럽게 올라가게
            float eased = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(startY, targetY, eased);
            transform.localPosition = pos;

            yield return null;
        }

        Vector3 finalPos = transform.localPosition;
        finalPos.y = targetY;
        transform.localPosition = finalPos;

        isHovering = true;
    }

    void Update()
    {
        if (!isHovering) return;

        Vector3 pos = transform.localPosition;
        pos.y = targetY + Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
        transform.localPosition = pos;
    }
}