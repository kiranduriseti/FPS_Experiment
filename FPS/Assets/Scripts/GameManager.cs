using UnityEngine;
using TMPro;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject defeatPanel; 
    [SerializeField] private GameObject Crosshair;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text ammoWarning;
    [SerializeField] private int targetKills = 20;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float displayTime = 1.5f;

    private float currentKills = 0;
    private float currentHealth = 0;
    private float maxHealth = 0;
    private float currentAmmo = 0;
    private float maxAmmo = 0;
    private bool gameEnded = false;

    private Coroutine ammoWarningRoutine;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Time.timeScale = 0f;
        Time.timeScale = 1f;

        //startPanel.SetActive(true);
        startPanel.SetActive(false);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);

        ammoWarning.gameObject.SetActive(false);
        
        //Crosshair.SetActive(false);
        Crosshair.SetActive(true);

        UpdateScoreUI();

        // if (cameraFollow != null)
        // {
        //     cameraFollow.DisableFPSCamera();
        // }
        if (cameraFollow != null)
        {
            cameraFollow.EnableFPSCamera();
        }
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        Crosshair.SetActive(true);
        Time.timeScale = 1f;

        if (cameraFollow != null)
        {
            cameraFollow.EnableFPSCamera();
        }
    }

    public void AddKill()
    {
        if (gameEnded) return;

        currentKills++;
        UpdateScoreUI();

        if (currentKills >= targetKills)
        {
            gameEnded = true;
            WinGame();
        }
    }

    public void updateHealth(float health)
    {
        currentHealth = health;
        if (maxHealth > 0f && (currentHealth / maxHealth) < 0.5f)
            healthText.color = Color.red;
        else
            healthText.color = Color.green;
        healthText.text = "Health: " + currentHealth + " / " + maxHealth;
    }

    public void updateAmmo(float ammo)
    {
        currentAmmo = ammo;
        if (maxAmmo > 0f && (currentAmmo / maxAmmo) < 0.5f)
            ammoText.color = Color.red;
        else
            ammoText.color = Color.orange;

        ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }

    public void setMaxHealth(float health)
    {
        maxHealth = health;
        UpdateScoreUI();
    }
    public void setMaxAmmo(float ammo)
    {
        maxAmmo = ammo;
    }

    private void WinGame()
    {
        Time.timeScale = 0f;
        scoreText.gameObject.SetActive(false);
        victoryPanel.SetActive(true);

        if (cameraFollow != null)
        {
            cameraFollow.DisableFPSCamera();
        }
    }

    public void LoseGame()
    {
        Time.timeScale = 0f;
        scoreText.gameObject.SetActive(false);
        defeatPanel.SetActive(true);

        if (cameraFollow != null)
        {
            cameraFollow.DisableFPSCamera();
        }
    }

    public void AmmoWarning()
    {
        ammoWarning.text = "Out of Ammo";

        if (ammoWarningRoutine != null)
        {
            StopCoroutine(ammoWarningRoutine);
        }

        ammoWarningRoutine = StartCoroutine(ShowAndHideRoutine());
    }

    private IEnumerator ShowAndHideRoutine()
    {
        ammoWarning.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        ammoWarning.gameObject.SetActive(false);
        ammoWarningRoutine = null;
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + currentKills + " / " + targetKills;
    }
}