using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealthUI : MonoBehaviour
{
    public GameObject uiPrefab;
    public Transform target;
    float visibleTime = 5f;
    float lastVisibleTime = 0f;
    Transform ui;
    Image healthSlider;
    Transform cam;
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        
        foreach(Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                ui = Instantiate(uiPrefab, c.transform).transform;
                healthSlider = ui.GetChild(0).GetComponent<Image>();
                ui.gameObject.SetActive(false);
                break;
            }
        }
        playerController = GetComponent<PlayerController>();
        playerController.OnHealthChanged += OnHealthChanged;
    }

    void OnHealthChanged(float maxHealth, float currHealth)
    {
        if (ui == null) return;
        lastVisibleTime = Time.time;
        ui.gameObject.SetActive(true);
        float healthPercent = currHealth/maxHealth;
        healthSlider.fillAmount = healthPercent;
        if (currHealth <= 0)
        {
            Destroy(ui.gameObject);
        }

    }

    void LateUpdate()
    {
        if (ui == null || target == null) return;

        ui.position = target.position;
        ui.forward = - cam.forward;
        if (Time.time - lastVisibleTime > visibleTime)
        {
            ui.gameObject.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
