using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Name")]
    [SerializeField] private string gunName;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject MuzzleFlash;
    [SerializeField] private Transform MuzzlePoint;
    [SerializeField] private GameObject worldPickupPrefab;
    [SerializeField] private Transform HoldPosition;

    [Header("Recoil")]
    [SerializeField] private float recoilAngle = 60f;
    [SerializeField] private float recoilSpeed = 100f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float recoilDirection = -1f;

    [Header("Sprint Pose Offsets")]
    [SerializeField] private Vector3 sprintLocalPositionOffset = new Vector3(0f, 0f, -0.1f);
    [SerializeField] private Vector3 sprintLocalRotationOffset = new Vector3(-60f, 0f, 15f);
    [SerializeField] private float poseLerpSpeed = 10f;

    [Header("Stats")]
    [SerializeField] private float fireRate = 0.75f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float ammo = 20f;
    [SerializeField] private float pickupAmmo = 2f;

    private Vector3 normalLocalPosition;
    private Quaternion normalLocalRotation;

    private Vector3 currentBasePosition;
    private Quaternion currentBaseRotation;

    private Quaternion recoilOffset = Quaternion.identity;
    private Quaternion targetRecoilOffset = Quaternion.identity;

    private bool isSprinting;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        normalLocalPosition = transform.localPosition;
        normalLocalRotation = transform.localRotation;

        ApplyDefaultStats();

        currentBasePosition = normalLocalPosition;
        currentBaseRotation = normalLocalRotation;
    }

    public void InitializeWeapon()
    {
        currentBasePosition = normalLocalPosition;
        currentBaseRotation = normalLocalRotation;

        recoilOffset = Quaternion.identity;
        targetRecoilOffset = Quaternion.identity;
    }

    private void ApplyDefaultStats()
    {
        if (gunName == "Default Primary")
        {
            sprintLocalPositionOffset = new Vector3(0f, 0f, -0.1f);
            sprintLocalRotationOffset = new Vector3(60f, 0f, -20f);
            normalLocalRotation = Quaternion.Euler(0f, 180f, 0f);

            recoilAngle = 70f;
            recoilSpeed = 100f;
            returnSpeed = 10f;

            damage = 40f;
            fireRate = 0.75f;
            ammo = 10f;
            pickupAmmo = 2f;
            
            recoilDirection = 1;
        }
        else if (gunName == "Default Secondary")
        {
            sprintLocalPositionOffset = new Vector3(-0.2f, 0f, -0.1f);
            sprintLocalRotationOffset = new Vector3(-60f, 0f, -20f);

            recoilAngle = 30f;
            recoilSpeed = 50f;
            returnSpeed = 20f;

            damage = 10f;
            fireRate = 0.4f;
            ammo = 20f;
            pickupAmmo = 10f;

            recoilDirection = -1;
        }
        else if (gunName == "Jetpack Gun")
        {
            sprintLocalPositionOffset = new Vector3(-0.2f, 0f, -0.1f);
            sprintLocalRotationOffset = new Vector3(-60f, 0f, -20f);

            recoilAngle = 80f;
            recoilSpeed = 100f;
            returnSpeed = 5f;

            damage = 100f;
            fireRate = 1.5f;
            ammo = 5f;
            pickupAmmo = 1f;

            recoilDirection = -1;
        }
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = isSprinting
            ? normalLocalPosition + sprintLocalPositionOffset
            : normalLocalPosition;

        float pitch = 0f;
        if (cameraTransform != null)
        {
            pitch = cameraTransform.localEulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;
        }

        Quaternion cameraPitchRotation = Quaternion.Euler(pitch, 0f, 0f);

        Quaternion sprintRotationOffset = isSprinting
            ? Quaternion.Euler(sprintLocalRotationOffset)
            : Quaternion.identity;

        Quaternion desiredRotation = cameraPitchRotation * normalLocalRotation * sprintRotationOffset;

        currentBasePosition = Vector3.Lerp(
            currentBasePosition,
            desiredPosition,
            poseLerpSpeed * Time.deltaTime
        );

        currentBaseRotation = Quaternion.Slerp(
            currentBaseRotation,
            desiredRotation,
            poseLerpSpeed * Time.deltaTime
        );

        targetRecoilOffset = Quaternion.Slerp(
            targetRecoilOffset,
            Quaternion.identity,
            returnSpeed * Time.deltaTime
        );

        recoilOffset = Quaternion.Slerp(
            recoilOffset,
            targetRecoilOffset,
            recoilSpeed * Time.deltaTime
        );

        transform.localPosition = currentBasePosition;
        transform.localRotation = currentBaseRotation * recoilOffset;
    }

    public void Fire()
    {
        targetRecoilOffset *= Quaternion.Euler(recoilDirection * recoilAngle, 0f, 0f);
    }

    public void SetGunActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }

    public float getDamage()
    {
        return damage;
    }

    public float getFireRate()
    {
        return fireRate;
    }

    public float getAmmo()
    {
        return ammo;
    }
    public GameObject getMuzzleFlash()
    {
        return MuzzleFlash;
    }
    public Transform getMuzzlePoint()
    {
        return MuzzlePoint;
    }
    public Transform getHoldPosition()
    {
        return HoldPosition;
    }

    public string getGunName()
    {
        return gunName;
    }

    public float getPickupAmmoAmount()
    {
        return pickupAmmo;
    }
    public GameObject GetWorldPickupPrefab()
    {
        return worldPickupPrefab;
    }
    
}