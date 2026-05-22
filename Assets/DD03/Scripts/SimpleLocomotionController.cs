using UnityEngine;

public class SimpleLocomotionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 90f;   // degrees per second

    private void Update()
    {
        // Left joystick X → turn
        float turnInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            float yaw = turnInput * turnSpeed * Time.deltaTime;
            PlayerRigRef.Instance.transform.Rotate(Vector3.up, yaw, Space.World);
        }

        // Right joystick Y → forward/back
        float moveInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 forward = new Vector3(PlayerRigRef.Instance.CenterEyeAnchor.forward.x,
                0, PlayerRigRef.Instance.CenterEyeAnchor.forward.z).normalized;
            Vector3 move = forward * moveInput * moveSpeed * Time.deltaTime;
            PlayerRigRef.Instance.transform.position += move;
        }
    }
}
