using UnityEngine;
using Unity.Mathematics;

public class SplineConnectToEgo : MonoBehaviour
{
    public enum Axis { X, Y, Z }

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

    [Header("Globe Surface Offset")]
    [Tooltip("Globe의 실제 Collider/Renderer 반지름을 자동으로 endOffset에 적용")]
    public bool autoGlobeSurfaceOffset = true;
    private float autoEndOffset = 0f;  // 런타임에 계산된 Globe 반지름

    [Header("Visibility")]
    public bool hideOnStart = true;

    [Header("Update")]
    public bool keepUpdating = true;

    [Header("Growth Animation")]
    [SerializeField] private GrowthRateController growthController;
    public bool playGrowthOnConnect = true;
    public bool resetGrowthOnDisconnect = true;

    private Transform targetGlobe;
    private bool connected = false;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;

        if (growthController == null)
            growthController = GetComponent<GrowthRateController>();

        if (hideOnStart)
            gameObject.SetActive(false);
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

        if (autoGlobeSurfaceOffset)
            autoEndOffset = GetGlobeRadius(globe);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);


        UpdateSpline();
        Debug.Log("[SplineConnectToEgo] 정상작동");
        growthController.PlayGrowth();
    }


    private float GetGlobeRadius(Transform globe)
    {
        // 1순위: SphereCollider (가장 정확)
        var sphere = globe.GetComponent<SphereCollider>();
        if (sphere != null)
            return sphere.radius * Mathf.Max(
                globe.lossyScale.x,
                globe.lossyScale.y,
                globe.lossyScale.z);

        // 2순위: Renderer bounds (Visual 기준)
        var rend = globe.GetComponent<Renderer>();
        if (rend != null)
            return rend.bounds.extents.magnitude * 0.57735f; // extents는 반대각선이므로 보정

        // 3순위: 수동 endOffset 폴백
        Debug.LogWarning($"[SplineConnect] {globe.name}에서 반지름을 찾을 수 없어 endOffset({endOffset})을 사용합니다.");
        return endOffset;
    }

    public void Disconnect()
    {
        connected = false;

        if (resetGrowthOnDisconnect && growthController != null)
            growthController.ResetGrowth();

        if (hideOnStart)
            gameObject.SetActive(false);
    }

    //다른 Globe로 즉시 재연결
    public void ReconnectToGlobe(Transform newGlobe)
    {
        if (growthController != null)
            growthController.ResetGrowth();

        ConnectToGlobe(newGlobe);
    }

    private void UpdateSpline()
    {
        Vector3 start = smallEgoCenter.position;
        Vector3 end = targetGlobe.position;
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.0001f) return;
        direction.Normalize();

        float appliedEndOffset = autoGlobeSurfaceOffset ? autoEndOffset : endOffset;

        Vector3 adjustedStart = start + direction * startOffset;
        Vector3 adjustedEnd = end - direction * endOffset;
        Vector3 adjustedDirection = adjustedEnd - adjustedStart;
        float adjustedDistance = adjustedDirection.magnitude;

        if (adjustedDistance <= 0.0001f) return;
        adjustedDirection.Normalize();

        float finalLength = Mathf.Min(adjustedDistance * lengthMultiplier, maxLength);

        transform.position = adjustedStart;
        RotateToDirection(adjustedDirection);
        ApplyScale(finalLength);
    }

    private void RotateToDirection(Vector3 direction)
    {
        transform.rotation = lengthAxis switch
        {
            Axis.X => Quaternion.FromToRotation(Vector3.right, direction),
            Axis.Y => Quaternion.FromToRotation(Vector3.up, direction),
            _ => Quaternion.FromToRotation(Vector3.forward, direction),
        };
    }

    private void ApplyScale(float finalLength)
    {
        Vector3 newScale = originalScale;
        float scaleAlongAxis = finalLength / Mathf.Max(originalLength, 0.0001f);

        if (lengthAxis == Axis.X) newScale.x = originalScale.x * scaleAlongAxis;
        else if (lengthAxis == Axis.Y) newScale.y = originalScale.y * scaleAlongAxis;
        else newScale.z = originalScale.z * scaleAlongAxis;

        transform.localScale = newScale;
    }
}