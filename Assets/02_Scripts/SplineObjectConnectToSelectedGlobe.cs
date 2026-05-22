using UnityEngine;

public class SplineObjectConnectToSelectedGlobe : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Header("References")]
    public Transform smallEgoCenter;

    [Header("Spline Axis")]
    public Axis lengthAxis = Axis.Y;
    public float originalLength = 1f;

    [Header("Length Control")]
    public float lengthMultiplier = 1f;
    public float maxLength = 3f;
    public float startOffset = 0f;
    public float endOffset = 0f;

    [Header("Visibility")]
    public bool hideOnStart = true;

    [Header("Update")]
    public bool keepUpdating = true;

    private Transform targetGlobe;
    private bool connected = false;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;

        if (hideOnStart)
        {
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (!connected || !keepUpdating) return;

        UpdateSpline();
    }

    public void ConnectToGlobe(Transform globe)
    {
        if (smallEgoCenter == null)
        {
            Debug.LogWarning("SmallEgo Center가 비어있습니다.");
            return;
        }

        if (globe == null)
        {
            Debug.LogWarning("Target Globe가 없습니다.");
            return;
        }

        targetGlobe = globe;
        connected = true;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        UpdateSpline();
    }

    private void UpdateSpline()
    {
        Vector3 start = smallEgoCenter.position;
        Vector3 end = targetGlobe.position;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.0001f) return;

        direction.Normalize();

        Vector3 adjustedStart = start + direction * startOffset;
        Vector3 adjustedEnd = end - direction * endOffset;

        Vector3 adjustedDirection = adjustedEnd - adjustedStart;
        float adjustedDistance = adjustedDirection.magnitude;

        if (adjustedDistance <= 0.0001f) return;

        adjustedDirection.Normalize();

        float finalLength = adjustedDistance * lengthMultiplier;
        finalLength = Mathf.Min(finalLength, maxLength);

        // 핵심: pivot이 시작점이므로 Spline 위치를 시작점에 둔다
        transform.position = adjustedStart;

        // 핵심: Spline의 길이축을 선택한 구 방향으로 회전
        RotateToDirection(adjustedDirection);

        // 핵심: 길이축 scale만 늘린다
        ApplyScale(finalLength);
    }

    private void RotateToDirection(Vector3 direction)
    {
        if (lengthAxis == Axis.X)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        }
        else if (lengthAxis == Axis.Y)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }
        else
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        }
    }

    private void ApplyScale(float finalLength)
    {
        Vector3 newScale = originalScale;

        float scaleAlongAxis = finalLength / Mathf.Max(originalLength, 0.0001f);

        if (lengthAxis == Axis.X)
        {
            newScale.x = originalScale.x * scaleAlongAxis;
        }
        else if (lengthAxis == Axis.Y)
        {
            newScale.y = originalScale.y * scaleAlongAxis;
        }
        else
        {
            newScale.z = originalScale.z * scaleAlongAxis;
        }

        transform.localScale = newScale;
    }
}