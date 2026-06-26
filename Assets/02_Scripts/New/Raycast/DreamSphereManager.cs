using Oculus.Interaction;
using System;
using UnityEngine;

public class DreamSphereManager : MonoBehaviour
{
   //Ray 관련 이벤트 총괄 매니저 
   //RayEventProvider가 구독 

    public static DreamSphereManager Instance { get; private set; }

    // Ray Event : Select
    public event Action<Transform> OnSphereSelected;
    public event Action<GameObject> OnSphereClicked;
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
        //Play Hover SFX 
        SoundManager.Instance.PlaySFXByIndex(2, volume: 0.4f);
        OnSpherehover?.Invoke(gameObject);
    }

    // Spline의 Growth Rate Controller, SplineAimFromEgo에게 전달 
    public void SendSphereInfo(Transform sphereTransform)
    {
        Debug.Log($"[DreamSphereManager] Sphere Grabbed: {sphereTransform.name}");
        OnSphereSelected?.Invoke(sphereTransform);
    }

    //이벤트 자체는 SendSphererInfo와 같으나 넘겨주는 정보가 다름 
    public void ClickSphereEvent(GameObject gameobject)
    {
        Debug.Log($"[DreamSphereManager] Sphere Grabbed: {gameobject.name}");
        //Click Event
        SoundManager.Instance.PlaySFXByIndex(3, volume: 0.4f);
        OnSphereClicked?.Invoke(gameobject);
    }

}
