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
    public SplineConnectToEgo splineConnectToEgo;

    [Header("Options")]
    public bool hideOtherGlobesOnClick = true;
    public bool disableScaleControllerOnDetach = true;
    public string globeNamePrefix = "DreamGlobe_";


    private Transform Selectedsphere;
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

        SelectableObject selectable = obj.GetComponent<SelectableObject>();
        Selectedsphere = obj.transform;
        
        isClicked = true;

        //Grab 이동 방지 
        Transform isdk = transform.Find("ISDK_RayGrabInteraction");
        if (isdk != null) isdk.gameObject.SetActive(false);

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

            if (splineGrowthController != null)  
            StartCoroutine(PlaySplineGrowthAfterDelay());

            ImageSetData currentSetData = selectable.GetImageSet();
            EmotionParticlePlayer.Instance.PlayParticleForSet(currentSetData);
            Debug.Log("파티클 재생 실시");

    }
    private IEnumerator PlaySplineGrowthAfterDelay()
    {
        yield return new WaitForSeconds(splineGrowthDelay);

        if (splineConnectToEgo!= null)
        {
            splineConnectToEgo.ConnectToGlobe(Selectedsphere);
        }
    }

}