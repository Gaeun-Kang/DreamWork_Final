using UnityEngine;
using System.Collections;

public class DreamGlobeClickDetach : MonoBehaviour
{
    [Header("References")]
    public AlembicSyncPlayer alembicSyncPlayer;
    public Transform pointRoot;

    [Header("Material Target")]
    public Renderer[] targetRenderers; // SmallEgo 쪽 Renderer들 넣기

    [Header("Spline Object Connection")]
    public SplineObjectConnectToSelectedGlobe splineObjectConnector;

    [Header("Spline Growth")]
    public GrowthRateController splineGrowthController;
    public float splineGrowthDelay = 1.5f;

    [Header("Options")]
    public bool detachOnClick = true;
    public bool hideOtherGlobesOnClick = true;
    public bool disableScaleControllerOnDetach = true;
    public string globeNamePrefix = "DreamGlobe_";

    private bool isClicked = false;


    public void SelectGlobe()
    {
        if (isClicked) return;
        isClicked = true;

        Debug.Log($"Clicked Globe: {gameObject.name}");

        // 1. Alembic 정지
        if (alembicSyncPlayer != null)
        {
            alembicSyncPlayer.Pause();
        }
        else
        {
            Debug.LogWarning("AlembicSyncPlayer가 연결되지 않았습니다.");
        }

        /* 2. 클릭한 Globe의 material을 SmallEgo에 적용
        ApplySelectedMaterialToTargets();

        if (splineObjectConnector != null)
        {
            splineObjectConnector.ConnectToGlobe(transform);
        }
        if (splineGrowthController != null)
        {
            StartCoroutine(PlaySplineGrowthAfterDelay());
        }
        */

        // 3. 현재 월드 트랜스폼 저장
        Vector3 worldPosition = transform.position;
        Quaternion worldRotation = transform.rotation;
        Vector3 worldScale = transform.lossyScale;

        // 4. 부모에서 분리
        if (detachOnClick)
        {
            transform.SetParent(null, true);
            transform.position = worldPosition;
            transform.rotation = worldRotation;
            transform.localScale = worldScale;
        }

        // 5. Scale 제어 중지
        if (disableScaleControllerOnDetach)
        {
            DreamGlobeScaleByCenterDistance scaler =
                GetComponent<DreamGlobeScaleByCenterDistance>();

            if (scaler != null)
            {
                scaler.enabled = false;
            }
        }

        // 6. 나머지 Globe 숨김
        if (hideOtherGlobesOnClick)
        {
            HideOtherGlobes();
        }
    }
    private IEnumerator PlaySplineGrowthAfterDelay()
    {
        yield return new WaitForSeconds(splineGrowthDelay);

        if (splineGrowthController != null)
        {
            splineGrowthController.ResetGrowth();
            splineGrowthController.PlayGrowth();
        }
    }
    private void ApplySelectedMaterialToTargets()
    {
        Renderer sourceRenderer = GetComponentInChildren<Renderer>();

        if (sourceRenderer == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Renderer가 없습니다.");
            return;
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.LogWarning("SmallEgo에 적용할 Target Renderer가 없습니다.");
            return;
        }

        Material selectedMaterial = sourceRenderer.material;

        foreach (Renderer target in targetRenderers)
        {
            if (target == null) continue;

            target.material = selectedMaterial;
        }

        Debug.Log($"Selected material applied to SmallEgo: {selectedMaterial.name}");
    }

    private void HideOtherGlobes()
    {
        if (pointRoot == null)
        {
            Debug.LogWarning("Point Root가 비어있어서 다른 Globe를 숨길 수 없습니다.");
            return;
        }

        DreamGlobeClickDetach[] allGlobes =
            pointRoot.GetComponentsInChildren<DreamGlobeClickDetach>(true);

        foreach (DreamGlobeClickDetach globe in allGlobes)
        {
            if (globe == this) continue;
            globe.gameObject.SetActive(false);
        }
    }
}