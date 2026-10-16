using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform pivot;
    public float sensitivity = 0.2f;
    private Vector2 lookInput;
    private float yaw = 0f;
    private float pitch = 0f;

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        yaw += lookInput.x * sensitivity;
        pitch -= lookInput.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -45f, 60f);

        pivot.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }
}
