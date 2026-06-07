using Oculus.Interaction;
using System;
using System.Security.Cryptography;
using UnityEngine;

public class RayEventProvider : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private SelectableObject selectableObject;
    [SerializeField] private MagicBalllVisble magicballVisble;
    [SerializeField] private PortallMaterialController portallMaterialController;


    void Awake()
    {
        if(selectableObject == null) selectableObject = GetComponent<SelectableObject>();

        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
            Debug.LogError("[RayEventProvider] Grabbable 컴포넌트를 찾을 수 없습니다.");
        }

        GameObject targetobj = GameObject.FindWithTag("Portal");
        if (portallMaterialController == null) 
        {
            portallMaterialController = targetobj.GetComponent<PortallMaterialController>();
            magicballVisble = targetobj.GetComponent<MagicBalllVisble>();
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

    public void SetPortalController(PortallMaterialController controller)
    {
        portallMaterialController = controller;
    }

    //send own transform info to DreamSphererManager 
    private void SphereselectedEvent(PointerEvent pointerEvent)
    {
        switch(pointerEvent.Type)
        {

            case PointerEventType.Hover:
                DreamSphereManager.Instance.HoverSphereinfo(this.gameObject);
                break;

            case PointerEventType.Select:
                ImageSetData imageSet = selectableObject.GetImageSet();

                if (imageSet == null)
                {
                    Debug.LogWarning(
                        $"[GlobeSelectionBridge] '{gameObject.name}' — ImageSet을 가져오지 못했습니다. " +
                        "SetGlobeIndex()가 호출되었는지, 해당 인덱스가 있는지 확인하세요.", this);
                    return;
                }

                portallMaterialController.ApplyRelateImage(imageSet);
                magicballVisble.ShrinkVFX();
                DreamSphereManager.Instance.SendSphereInfo(this.transform);
                DreamSphereManager.Instance.ClickSphereEvent(this.gameObject);

                this.transform.localPosition = Vector3.zero;
                this.gameObject.transform.localScale = Vector3.one;

                break;
        }

    }

}
