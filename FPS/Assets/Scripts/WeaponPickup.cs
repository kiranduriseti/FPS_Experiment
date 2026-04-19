using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject heldGunPrefab;
    [SerializeField] private float resetRadius = 2.5f;
    [SerializeField] private float spinSpeed = 90f;

    private bool canPickUp = true;
    private Transform blockedPlayer;
    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        if (blockedPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, blockedPlayer.position);
            if (dist > resetRadius)
            {
                canPickUp = true;
                blockedPlayer = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canPickUp)
            return;

        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        canPickUp = false;
        blockedPlayer = player.transform;

        player.SwapCurrentWeapon(gameObject, heldGunPrefab);
    }

    public void SetPickupCooldownPlayer(Transform player)
    {
        canPickUp = false;
        blockedPlayer = player;
    }
}