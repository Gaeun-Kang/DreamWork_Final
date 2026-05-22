using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterControllerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 3f;
    public float gravity = -9.81f;
    public float groundStickForce = -2f;

    [Header("Mouse Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 120f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private float pitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                playerCamera = cam.transform;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
#else
        Vector2 mouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
#endif

#if ENABLE_INPUT_SYSTEM
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;
#else
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;
#endif

        // 좌우 회전: 플레이어 몸 전체 회전
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전: 카메라만 회전
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void HandleMovement()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 input = new Vector2(
            Keyboard.current[Key.A].isPressed ? -1f : Keyboard.current[Key.D].isPressed ? 1f : 0f,
            Keyboard.current[Key.S].isPressed ? -1f : Keyboard.current[Key.W].isPressed ? 1f : 0f
        );
#else
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
#endif

        // 중요: 월드 기준이 아니라 플레이어가 바라보는 방향 기준으로 이동
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move.y = 0f;
        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * movementSpeed * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundStickForce;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}