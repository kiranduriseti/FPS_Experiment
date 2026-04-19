using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 1f;

    private float pitch = 0f;
    private bool cameraEnabled = false;

    private void Start()
    {
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        mouseSensitivity = 1.5f;

        cameraEnabled = true;
        //cameraEnabled = false;
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