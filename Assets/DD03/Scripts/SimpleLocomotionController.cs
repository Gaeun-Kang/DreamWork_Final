using UnityEngine;

public class SimpleLocomotionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 90f;   // degrees per second
    private float moveInput;


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

    private void FixedUpdate()
    {

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // 1. 시선 방향 계산 (y축 회전만 적용)
            Vector3 forward = new Vector3(PlayerRigRef.Instance.CenterEyeAnchor.forward.x, 0, PlayerRigRef.Instance.CenterEyeAnchor.forward.z).normalized;
            Vector3 move = forward * moveInput * moveSpeed * Time.fixedDeltaTime;
            // 3. Rigidbody를 통한 이동 처리
            rb.MovePosition(rb.position + move);

        }

    }


    private void Update()
    {
        // Right joystick Y → forward/back
        moveInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        
     }
}
