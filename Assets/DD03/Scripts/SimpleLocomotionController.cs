using Oculus.Interaction;
using UnityEngine;

public class SimpleLocomotionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 90f;   // degrees per second
 
    [Header("Ray Interaction")]
    [SerializeField] private RayInteractor rayInteractor; 

    private float moveInput;
    private bool isMoving = false;

    //simple 이동 스크립트, 현재 오른쪽 컨트롤러만 사용하기로 협의 


    void BtnDown()
    {
        if(OVRInput.GetDown(OVRInput.Button.One))
        {
            //A 버튼 UI 띄우기 

        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            //B 버튼 UI 띄우기 

        }

    }
    private void Update()
    {
        moveInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;

        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(moveInput) > 0.1f;

        // 상태 변화 시에만 토글 (매 프레임 SetActive 방지)
        if (wasMoving != isMoving)
        {
            SetRayActive(!isMoving);
        }
    }

    private void SetRayActive(bool active)
    {
        if (rayInteractor == null) return;
        rayInteractor.gameObject.SetActive(active);
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Vector3 forward = new Vector3(
            PlayerRigRef.Instance.CenterEyeAnchor.forward.x, 0,
            PlayerRigRef.Instance.CenterEyeAnchor.forward.z
        ).normalized;

        Vector3 move = forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
