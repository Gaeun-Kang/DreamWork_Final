using UnityEngine;
using UnityEngine.InputSystem;

public class DreamGlobeMouseRaycaster : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Raycast")]
    public float maxDistance = 1000f;
    public LayerMask raycastMask = ~0;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (!WasPressedThisFrame()) return;

        if (targetCamera == null)
        {
            Debug.LogWarning("Target Camera가 없습니다. MainCamera 태그 또는 직접 할당을 확인하세요.");
            return;
        }

        Vector2 screenPosition = GetPointerPosition();

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, raycastMask);

        if (hits.Length == 0)
        {
            Debug.Log("Raycast hit 없음");
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            DreamGlobeClickDetach globe =
                hit.collider.GetComponentInParent<DreamGlobeClickDetach>();

            if (globe != null)
            {
                Debug.Log($"Ray hit globe: {globe.name}");
                globe.SelectGlobe();
                return;
            }
        }

        Debug.Log($"Raycast는 맞았지만 Globe 아님: {hits[0].collider.name}");
    }

    private bool WasPressedThisFrame()
    {
        bool mousePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool touchPressed =
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        return mousePressed || touchPressed;
    }

    private Vector2 GetPointerPosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        return Vector2.zero;
    }
}