using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 1f;

    [Header("Camera Bob")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmount = 0.03f;
    [SerializeField] private float sprintBobSpeed = 16f;
    [SerializeField] private float sprintBobAmount = 0.06f;
    [SerializeField] private float bobSmoothSpeed = 8f;

    private float pitch = 0f;
    private bool cameraEnabled = false;

    private Vector3 startLocalPos;
    private float bobTimer;
    private bool isMoving;
    private bool isSprinting;

    private void Start()
    {
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        startLocalPos = transform.localPosition;

        mouseSensitivity = 1.5f;

        cameraEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (!cameraEnabled) return;
        if (Mouse.current == null) return;
        if (playerBody == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float yawInput = mouseDelta.x * mouseSensitivity;
        float pitchInput = mouseDelta.y * mouseSensitivity;

        pitch -= pitchInput;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        Vector3 bodyEuler = playerBody.eulerAngles;
        bodyEuler.y += yawInput;
        playerBody.eulerAngles = bodyEuler;

        HandleCameraBob();
    }

    private void HandleCameraBob()
    {
        if (!isMoving)
        {
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startLocalPos,
                bobSmoothSpeed * Time.deltaTime
            );
            return;
        }

        float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
        float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;

        float xOffset = Mathf.Cos(bobTimer * 0.5f) * bobAmount;
        float yOffset = Mathf.Sin(bobTimer) * bobAmount;

        Vector3 targetPos = startLocalPos + new Vector3(xOffset, yOffset, 0f);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            bobSmoothSpeed * Time.deltaTime
        );
    }

    public void SetMovementState(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;
    }

    public void EnableFPSCamera()
    {
        cameraEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisableFPSCamera()
    {
        cameraEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}