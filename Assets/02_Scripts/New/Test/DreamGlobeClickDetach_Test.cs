using UnityEngine;
using System.Collections;

public class DreamGlobeClickDetach_Test : MonoBehaviour
{

    [Header("References")]
    public AlembicSyncPlayer alembicSyncPlayer;
    public Transform pointRoot;

    [Header("Spline Growth")]
    public GrowthRateController splineGrowthController;
    public float splineGrowthDelay = 1.5f;
    public SplineObjectConnectToSelectedGlobe splineObjectConnector;

    [Header("Options")]
    public bool detachOnSelected = true;
    public bool hideOtherGlobesOnClick = true;
    public bool disableScaleControllerOnDetach = true;
    public string globeNamePrefix = "DreamGlobe_";

    private bool isClicked = false;

    private void OnEnable()
    {
        DreamSphereManager.Instance.OnSphereClicked += SelectGlobe;
    }

    private void OnDisable()
    {
        DreamSphereManager.Instance.OnSphereClicked -= SelectGlobe;
    }

    public void SelectGlobe(GameObject obj)
    {
        if (isClicked) return;
        if (obj != this.gameObject) return;
        isClicked = true;

        Vector3 targetWorldScale = transform.lossyScale;
        Vector3 targetWorldPosition = transform.position;
        Quaternion targetWorldRotation = transform.rotation;

        if (alembicSyncPlayer != null) alembicSyncPlayer.Pause();
        else Debug.LogWarning("AlembicSyncPlayer가 연결되지 않았습니다.");

        if (disableScaleControllerOnDetach)
        {
            DreamGlobeScaleByCenterDistance scaler =
                GetComponent<DreamGlobeScaleByCenterDistance>();

            if (scaler != null)
            {
                scaler.lockScale = true;
                scaler.enabled = false;
                Destroy(scaler);
            }
        }

        if (detachOnSelected)
        {
            transform.SetParent(null, true);

            transform.position = targetWorldPosition;
            transform.rotation = targetWorldRotation;
            transform.localScale = targetWorldScale; 
        }


        if (splineGrowthController != null)  
            StartCoroutine(PlaySplineGrowthAfterDelay());

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

}