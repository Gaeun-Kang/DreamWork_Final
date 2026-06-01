using Oculus.Interaction;
using System;
using UnityEngine;

public class DreamSphereManager : MonoBehaviour
{
   //DreamSphere <-> Spline 간 이벤트 관리 매니저
   //RayEventProvider가 구독 

    public static DreamSphereManager Instance { get; private set; }

    // Ray Event : Select
    public event Action<Transform> OnSphereSelected;
    public event Action<GameObject> OnSpherehover;


    void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HoverSphereinfo(GameObject gameObject)
    {
        Debug.Log($"현재 겹쳐져있는 오브젝트:{gameObject.name}");
        OnSpherehover?.Invoke(gameObject);
    }

    // Spline의 Growth Rate Controller, SplineAimFromEgo에게 전달 
    public void SendSphereInfo(Transform sphereTransform)
    {
        Debug.Log($"[DreamSphereManager] Sphere Grabbed: {sphereTransform.name}");
        OnSphereSelected?.Invoke(sphereTransform);
    }

}
