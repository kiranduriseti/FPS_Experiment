using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [SerializeField] private Button startButton;

    [SerializeField] private int targetKills = 60;
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
        //Testing
        StartGame();
        return;
        Time.timeScale = 0f;

        if (startPanel != null) startPanel.SetActive(true);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (ammoWarning != null) ammoWarning.gameObject.SetActive(false);
        if (Crosshair != null) Crosshair.SetActive(false);

        if (ammoText != null) ammoText.gameObject.SetActive(false);
        if (healthText != null) healthText.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        UpdateScoreUI();

        if (cameraFollow != null)
        {
            cameraFollow.DisableFPSCamera();
        }

        if (startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (Crosshair != null) Crosshair.SetActive(true);

        if (ammoText != null) ammoText.gameObject.SetActive(true);
        if (healthText != null) healthText.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        Time.timeScale = 1f;

        if (cameraFollow != null)
        {
            cameraFollow.EnableFPSCamera();
        }

        yield return new WaitUntil(() => !Input.GetMouseButton(0));
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
            ammoText.color = new Color(1f, 0.5f, 0f);

        ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }

    public void setMaxHealth(float health)
    {
        maxHealth = health;
    }

    public void setMaxAmmo(float ammo)
    {
        maxAmmo = ammo;
    }

    public void WinGame()
    {
        Time.timeScale = 0f;

        if (scoreText != null) scoreText.gameObject.SetActive(false);

        victoryPanel.SetActive(true);

        if (cameraFollow != null)
        {
            cameraFollow.DisableFPSCamera();
        }
    }

    public void LoseGame()
    {
        Time.timeScale = 0f;

        if (scoreText != null) scoreText.gameObject.SetActive(false);

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
        yield return new WaitForSecondsRealtime(displayTime);
        ammoWarning.gameObject.SetActive(false);
        ammoWarningRoutine = null;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentKills + " / " + targetKills;
        }
    }
}