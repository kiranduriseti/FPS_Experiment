using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float primaryCastSpeed = 0.75f;
    [SerializeField] private float secondaryCastSpeed = 0.25f;
    [SerializeField] private float shootRange = 100f;

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerAnim anim;
    [SerializeField] private PlayerGun primaryGun;
    [SerializeField] private PlayerGun secondaryGun;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float bulletHoleLifetime = 5f;
    [SerializeField] private GameObject primaryMuzzleFlash;
    [SerializeField] private GameObject secondaryMuzzleFlash;
    [SerializeField] private float flashTime = 0.1f;

    [Header("Health")]
    [SerializeField] public float health = 100f;
    public float maxHealth;

    [Header("Layers")]
    [SerializeField] private LayerMask shootMask = ~0;

    [Header("Transforms")]
    [SerializeField] private Transform primaryMuzzlePoint;
    [SerializeField] private Transform secondaryMuzzlePoint;
    [SerializeField] private Transform gunHoldParent;

    private float primaryDamage;
    private float secondaryDamage;

    private float primaryFireRate;
    private float secondaryFireRate;

    private float primaryAmmo, secondaryAmmo;
    private float primaryMaxAmmo, secondaryMaxAmmo;

    private bool canCast = true;
    private bool castPressed;
    private MovementControls controls;
    private Dictionary<string, float> savedAmmo = new Dictionary<string, float>();
    private Dictionary<string, float> savedMaxAmmo = new Dictionary<string, float>();

    private bool primary = true;
    private bool secondary = false;
    private bool sprint = false;
    public event System.Action<float, float> OnHealthChanged;

    private void Awake()
    {
        if (controls == null)
            controls = new MovementControls();
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new MovementControls();

        controls.Enable();
        controls.PlayerMap.Cast.performed += OnCast;
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.PlayerMap.Cast.performed -= OnCast;
        controls.Disable();
    }

    private void Start()
    {
        primary = true;
        secondary = false;
        primaryGun.SetGunActive(true);
        secondaryGun.SetGunActive(false);

        primaryDamage = primaryGun.getDamage();
        primaryFireRate = primaryGun.getFireRate();

        secondaryDamage = secondaryGun.getDamage();
        secondaryFireRate = secondaryGun.getFireRate();

        primaryAmmo = primaryGun.getAmmo();
        secondaryAmmo = secondaryGun.getAmmo();

        primaryMaxAmmo = primaryAmmo;
        secondaryMaxAmmo = secondaryAmmo;

        primaryMuzzlePoint = primaryGun.getMuzzlePoint();
        secondaryMuzzlePoint = secondaryGun.getMuzzlePoint();

        primaryMuzzleFlash = primaryGun.getMuzzleFlash();
        secondaryMuzzleFlash = secondaryGun.getMuzzleFlash();
        
        maxHealth = health;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.setMaxHealth(maxHealth);
            GameManager.Instance.setMaxAmmo(primaryMaxAmmo);
            GameManager.Instance.updateHealth(health);
            GameManager.Instance.updateAmmo(primaryAmmo);
        }

        if (cam == null)
            cam = Camera.main;

        if (anim == null)
            anim = GetComponentInChildren<PlayerAnim>();

        OnHealthChanged?.Invoke(maxHealth, health);

        SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);
        SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);
    }

    private void OnCast(InputAction.CallbackContext context)
    {
        castPressed = true;
    }

    private void Update()
    {
        if (health <= 0f)
            return;

        if (castPressed)
        {
            castPressed = false;
            FireWeapon();
        }
    }

    private void FireWeapon()
    {
        if (!canCast || cam == null || sprint)
            return;
        
        if ((primary && primaryAmmo <= 0) || (secondary && secondaryAmmo <= 0))
        {
            GameManager.Instance.AmmoWarning();
            return;
        }

        if (primaryMuzzleFlash != null && primary)
        {
            Vector3 flashPos = primaryMuzzlePoint != null ? primaryMuzzlePoint.position : cam.transform.position;
            Quaternion flashRot = primaryMuzzlePoint != null ? primaryMuzzlePoint.rotation : cam.transform.rotation;

            GameObject flash = Instantiate(primaryMuzzleFlash, flashPos, flashRot);
            Destroy(flash, flashTime);
        }
        if (secondaryMuzzleFlash != null && secondary)
        {
            Vector3 flashPos = secondaryMuzzlePoint != null ? secondaryMuzzlePoint.position : cam.transform.position;
            Quaternion flashRot = secondaryMuzzlePoint != null ? secondaryMuzzlePoint.rotation : cam.transform.rotation;

            GameObject flash = Instantiate(secondaryMuzzleFlash, flashPos, flashRot);
            Destroy(flash, flashTime);
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, shootMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyController meleeEnemy = hit.collider.GetComponentInParent<EnemyController>();
                EnemyRangedController rangedEnemy = hit.collider.GetComponentInParent<EnemyRangedController>();

                if (meleeEnemy != null)
                {
                    if (primary)
                    {
                        meleeEnemy.enemyhit(primaryDamage);
                    }
                    else if (secondary)
                    {
                        meleeEnemy.enemyhit(secondaryDamage);
                    }
                }
                else if (rangedEnemy != null)
                {
                    if (primary)
                    {
                        rangedEnemy.enemyhit(primaryDamage);
                    }
                    else if (secondary)
                    {
                        rangedEnemy.enemyhit(secondaryDamage);
                    }
                }
            }
            else if (bulletHolePrefab != null)
            {
                int groundLayer = LayerMask.NameToLayer("Ground");
                int defaultLayer = LayerMask.NameToLayer("Default");

                if (hit.collider.gameObject.layer == groundLayer)
                {
                    Vector3 holePos = hit.point + hit.normal * 0.01f;

                    GameObject hole = Instantiate(
                        bulletHolePrefab,
                        holePos,
                        Quaternion.LookRotation(hit.normal)
                    );

                    Destroy(hole, bulletHoleLifetime);
                }
            }
        }

        if (anim != null)
        {
            anim.setTrigger("Cast");
        }

        if (primary && primaryGun != null)
        {
            primaryGun.Fire();
            primaryAmmo--;
            SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);
            GameManager.Instance.updateAmmo(primaryAmmo);
        }
        else if (secondary && secondaryGun != null)
        {
            secondaryGun.Fire();
            secondaryAmmo--;
            SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);
            GameManager.Instance.updateAmmo(secondaryAmmo);
        }

        if(primary == true)
        {
            StartCoroutine(ResetCastTimer(primaryCastSpeed));
        }
        else if (secondary == true)
        {
            StartCoroutine(ResetCastTimer(secondaryCastSpeed));
        }
    }

    public void PlayerHit(float damage = 10f)
    {
        if (health <= 0f)
            return;

        health -= damage;
        health = Mathf.Max(health, 0f);

        OnHealthChanged?.Invoke(maxHealth, health);

        GameManager.Instance.updateHealth(health);

        if (health <= 0f)
        {
            if (anim != null)
            {
                anim.SetPlayerDead(true);
            }

            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            Invoke(nameof(DelayedAction), 4.0f);
        }
    }

    public void playerhit(float damage = 10f)
    {
        PlayerHit(damage);
    }

    private void DelayedAction()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }
    }

    private IEnumerator ResetCastTimer(float castSpeed)
    {
        canCast = false;
        if(primary) yield return new WaitForSeconds(primaryFireRate);
        else if (secondary) yield return new WaitForSeconds(secondaryFireRate);
        canCast = true;
    }

    public void switchWeapon(float pos)
    {
        if (pos == 1 && primary) return;
        if (pos == 2 && secondary) return;

        if (pos == 1)
        {
            primary = true;
            secondary = false;
            primaryGun.SetGunActive(true);
            secondaryGun.SetGunActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.setMaxAmmo(primaryMaxAmmo);
                GameManager.Instance.updateAmmo(primaryAmmo);
            }
        }

        else if (pos == 2)
        {
            primary = false;
            secondary = true;
            secondaryGun.SetGunActive(true);
            primaryGun.SetGunActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.setMaxAmmo(secondaryMaxAmmo);
                GameManager.Instance.updateAmmo(secondaryAmmo);
            }
        }
    }

    public void sprintAnim(bool s)
    {
        sprint = s;
        if (primary)
        {
            primaryGun.SetSprinting(s);
        }
        if (secondary)
        {
            secondaryGun.SetSprinting(s);
        }
    }

    public void AddAmmoToCurrentWeapon()
    {
        if (primary)
        {
            float amount = primaryGun.getPickupAmmoAmount();
            primaryAmmo += amount;
            if (primaryAmmo > primaryMaxAmmo)
                primaryAmmo = primaryMaxAmmo;

            SaveWeaponAmmo(primaryGun, primaryAmmo, primaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(primaryAmmo);
        }
        else if (secondary)
        {
            float amount = secondaryGun.getPickupAmmoAmount();
            secondaryAmmo += amount;
           
            if (secondaryAmmo > secondaryMaxAmmo)
                secondaryAmmo = secondaryMaxAmmo;

            SaveWeaponAmmo(secondaryGun, secondaryAmmo, secondaryMaxAmmo);

            if (GameManager.Instance != null)
                GameManager.Instance.updateAmmo(secondaryAmmo);
        }
    }

    public void Heal(float amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;

        OnHealthChanged?.Invoke(maxHealth, health);

        if (GameManager.Instance != null)
            GameManager.Instance.updateHealth(health);
    }

    public bool NeedsHealth()
    {
        return health < maxHealth;
    }

    public bool NeedsAmmo()
    {
        if (primary) return primaryAmmo < primaryMaxAmmo;
        else if (secondary) return secondaryAmmo < secondaryMaxAmmo;
        return true;
    }

    public void SwapCurrentWeapon(GameObject touchedPickupObject, GameObject newHeldGunPrefab)
    {
        // PlayerGun pickup = newHeldGunPrefab != null
        // ? newHeldGunPrefab.GetComponent<PlayerGun>()
        // : null;

        if (newHeldGunPrefab == null)
            return;

        //gunHoldParent = pickup.getHoldPosition();
    
        Vector3 dropPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        if (primary)
        {
            PlayerGun oldGun = primaryGun;

            if (touchedPickupObject != null)
                Destroy(touchedPickupObject);

            if (oldGun != null)
            {
                SaveWeaponAmmo(oldGun, primaryAmmo, primaryMaxAmmo);

                GameObject oldPickupPrefab = oldGun.GetWorldPickupPrefab();

                if (oldPickupPrefab != null)
                {
                    GameObject dropped = Instantiate(oldPickupPrefab, dropPos, Quaternion.identity);

                    WeaponPickup droppedPickup = dropped.GetComponent<WeaponPickup>();
                    if (droppedPickup != null)
                        droppedPickup.SetPickupCooldownPlayer(transform);
                }

                Destroy(oldGun.gameObject);
            }

            GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
            newGunObj.transform.localPosition = Vector3.zero;
            newGunObj.transform.localRotation = Quaternion.identity;

            primaryGun = newGunObj.GetComponent<PlayerGun>();
            primaryGun.InitializeWeapon();

            primaryDamage = primaryGun.getDamage();
            primaryFireRate = primaryGun.getFireRate();

            LoadWeaponAmmo(primaryGun, primaryGun.getAmmo(), out primaryAmmo, out primaryMaxAmmo);

            primaryMuzzlePoint = primaryGun.getMuzzlePoint();
            primaryMuzzleFlash = primaryGun.getMuzzleFlash();

            primaryGun.SetGunActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.setMaxAmmo(primaryMaxAmmo);
                GameManager.Instance.updateAmmo(primaryAmmo);
            }
        }
        else if (secondary)
        {
            PlayerGun oldGun = secondaryGun;

            if (touchedPickupObject != null)
                Destroy(touchedPickupObject);

            if (oldGun != null)
            {
                SaveWeaponAmmo(oldGun, secondaryAmmo, secondaryMaxAmmo);

                GameObject oldPickupPrefab = oldGun.GetWorldPickupPrefab();

                if (oldPickupPrefab != null)
                {
                    GameObject dropped = Instantiate(oldPickupPrefab, dropPos, Quaternion.identity);

                    WeaponPickup droppedPickup = dropped.GetComponent<WeaponPickup>();
                    if (droppedPickup != null)
                        droppedPickup.SetPickupCooldownPlayer(transform);
                }

                Destroy(oldGun.gameObject);
            }

            GameObject newGunObj = Instantiate(newHeldGunPrefab, gunHoldParent);
            newGunObj.transform.localPosition = Vector3.zero;
            newGunObj.transform.localRotation = Quaternion.identity;

            secondaryGun = newGunObj.GetComponent<PlayerGun>();
            secondaryGun.InitializeWeapon();

            secondaryDamage = secondaryGun.getDamage();
            secondaryFireRate = secondaryGun.getFireRate();

            LoadWeaponAmmo(secondaryGun, secondaryGun.getAmmo(), out secondaryAmmo, out secondaryMaxAmmo);

            secondaryMuzzlePoint = secondaryGun.getMuzzlePoint();
            secondaryMuzzleFlash = secondaryGun.getMuzzleFlash();

            secondaryGun.SetGunActive(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.setMaxAmmo(secondaryMaxAmmo);
                GameManager.Instance.updateAmmo(secondaryAmmo);
            }
        }
    }
    private void SaveWeaponAmmo(PlayerGun gun, float currentAmmoValue, float maxAmmoValue)
    {
        if (gun == null) return;

        string gunName = gun.getGunName();
        savedAmmo[gunName] = currentAmmoValue;
        savedMaxAmmo[gunName] = maxAmmoValue;
    }

    private void LoadWeaponAmmo(PlayerGun gun, float defaultAmmo, out float currentAmmoValue, out float maxAmmoValue)
    {
        currentAmmoValue = defaultAmmo;
        maxAmmoValue = defaultAmmo;

        if (gun == null) return;

        string gunName = gun.getGunName();

        if (savedAmmo.TryGetValue(gunName, out float storedAmmo))
            currentAmmoValue = storedAmmo;

        if (savedMaxAmmo.TryGetValue(gunName, out float storedMaxAmmo))
            maxAmmoValue = storedMaxAmmo;
    }
} 