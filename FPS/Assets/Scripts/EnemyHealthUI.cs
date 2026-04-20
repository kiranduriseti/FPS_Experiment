using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUi : MonoBehaviour
{
    public GameObject uiPrefab;
    public Transform target;
    float visibleTime = 5f;
    float lastVisibleTime = 0f;
    Transform ui;
    Image healthSlider;
    Transform cam;

    private EnemyController enemyController;
    private EnemyRangedController enemyRangedController;

    void Start()
    {
        if (target == null)
        {
            target = transform;
        }

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                ui = Instantiate(uiPrefab, c.transform).transform;
                healthSlider = ui.GetChild(0).GetComponent<Image>();
                ui.gameObject.SetActive(false);
                break;
            }
        }

        enemyController = GetComponent<EnemyController>();
        enemyRangedController = GetComponent<EnemyRangedController>();

        if (enemyController != null)
        {
            enemyController.OnHealthChanged += OnHealthChanged;
        }

        if (enemyRangedController != null)
        {
            enemyRangedController.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (enemyController != null)
        {
            enemyController.OnHealthChanged -= OnHealthChanged;
        }

        if (enemyRangedController != null)
        {
            enemyRangedController.OnHealthChanged -= OnHealthChanged;
        }
    }

    void OnHealthChanged(float maxHealth, float currHealth)
    {
        if (ui == null) return;

        lastVisibleTime = Time.time;
        ui.gameObject.SetActive(true);

        float healthPercent = currHealth / maxHealth;
        healthSlider.fillAmount = healthPercent;

        if (currHealth <= 0)
        {
            Destroy(ui.gameObject);
        }
    }

    void LateUpdate()
    {
        if (ui == null || target == null || cam == null) return;

        ui.position = target.position;
        ui.forward = -cam.forward;

        if (Time.time - lastVisibleTime > visibleTime)
        {
            ui.gameObject.SetActive(false);
        }
    }
}