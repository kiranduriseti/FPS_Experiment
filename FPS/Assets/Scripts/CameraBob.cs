using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmount = 0.03f;
    [SerializeField] private float sprintBobSpeed = 16f;
    [SerializeField] private float sprintBobAmount = 0.06f;
    [SerializeField] private float bobSmoothSpeed = 8f;

    private Vector3 startLocalPos;
    private float bobTimer;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    public void HandleBob(bool isMoving, bool isSprinting)
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
}