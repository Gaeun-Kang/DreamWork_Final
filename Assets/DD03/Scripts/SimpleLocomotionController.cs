using Oculus.Interaction;
using UnityEngine.SceneManagement;
using UnityEngine;
using static Oculus.Interaction.Context;
using System.Collections;

public class SimpleLocomotionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 2.0f;

    [Header("Ray Interaction")]
    [SerializeField] private RayInteractor HrayInteractor;
    [SerializeField] private RayInteractor CrayInteractor;


    private float moveInput;
    private bool isMoving = false;

    // 왼쪽 컨트롤러 명시
    private OVRInput.Controller leftController = OVRInput.Controller.LTouch;


    private void Start()
    {
        //최초에는 꺼두기 
        HrayInteractor.gameObject.SetActive(false);
        CrayInteractor.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 1. 이동 인풋 처리
        moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, leftController).y;
        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(moveInput) > 0.1f;

        // 상태 변화 시에만 토글
        if (wasMoving != isMoving)
        {
            SetRayActive(!isMoving);
        }

    }


    private void SetRayActive(bool active)
    {
        if (HrayInteractor == null || CrayInteractor == null) return;
        HrayInteractor.gameObject.SetActive(active);
        CrayInteractor.gameObject.SetActive(active);

    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        // PlayerRigRef나 CenterEyeAnchor가 Null인지 체크 필요
        if (PlayerRigRef.Instance == null || PlayerRigRef.Instance.CenterEyeAnchor == null) return;

        Vector3 forward = new Vector3(
            PlayerRigRef.Instance.CenterEyeAnchor.forward.x, 0,
            PlayerRigRef.Instance.CenterEyeAnchor.forward.z
        ).normalized;

        Vector3 move = forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
