using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float healAmount = 25f;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 120f;

    [Header("Float")]
    [SerializeField] private float floatHeight = 0.15f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Pickup Layer")]
    [SerializeField] private LayerMask playerLayer;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (!player.NeedsHealth())
            return;

        player.Heal(healAmount);

        Destroy(gameObject);
    }
}