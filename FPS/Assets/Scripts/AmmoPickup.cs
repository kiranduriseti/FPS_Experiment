using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField] private float spinSpeed = 120f;

    [Header("Pickup Layer")]
    [SerializeField] private LayerMask playerLayer;

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
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

        if (!player.NeedsAmmo()) return;

        player.AddAmmoToCurrentWeapon();

        Destroy(gameObject);
    }
}