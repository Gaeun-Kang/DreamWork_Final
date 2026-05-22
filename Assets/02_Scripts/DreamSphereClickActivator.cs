using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DreamSphereClickActivator : MonoBehaviour
{
    public Camera targetCamera;

    [System.Serializable]
    public class DreamSplinePair
    {
        public Transform dreamSphere;
        public GameObject spline;
    }

    public DreamSplinePair[] pairs;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        bool clicked = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;
#else
        if (Input.GetMouseButtonDown(0))
            clicked = true;
#endif

        if (!clicked) return;

#if ENABLE_INPUT_SYSTEM
        Vector2 mousePos = Mouse.current.position.ReadValue();
#else
        Vector2 mousePos = Input.mousePosition;
#endif

        Ray ray = targetCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform clickedObject = hit.transform;

            foreach (var pair in pairs)
            {
                if (pair.dreamSphere == clickedObject)
                {
                    ActivateSplineAndGrow(pair.spline);
                    break;
                }
            }
        }
    }

    void ActivateSplineAndGrow(GameObject splineObject)
    {
        if (splineObject == null) return;

        splineObject.SetActive(true);

        GrowthRateController growth = splineObject.GetComponent<GrowthRateController>();

        if (growth == null)
        {
            Debug.LogWarning($"{splineObject.name}에 GrowthRateController가 없습니다.");
            return;
        }

        growth.ResetGrowth();
        growth.PlayGrowth();
    }
}