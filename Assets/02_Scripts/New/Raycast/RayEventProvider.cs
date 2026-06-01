using Oculus.Interaction;
using System;
using System.Security.Cryptography;
using UnityEngine;

public class RayEventProvider : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Grabbable grabbable;

    void Awake()
    {
        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
            if (grabbable == null)
                Debug.LogError("[RayEventProvider] Grabbable 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += SphereselectedEvent;
        }
    }


    void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= SphereselectedEvent;

        }
    }

    //send own transform info to DreamSphererManager 
    private void SphereselectedEvent(PointerEvent pointerEvent)
    {
        switch(pointerEvent.Type)
        {

            case PointerEventType.Hover:
                Debug.Log("Checking DreamSphere");
                DreamSphereManager.Instance.HoverSphereinfo(this.gameObject);
                break;

            case PointerEventType.Select:
                Debug.Log("pick up DreamSphere");
                DreamSphereManager.Instance.SendSphereInfo(this.transform);
                break;

          
        }

    }

}
