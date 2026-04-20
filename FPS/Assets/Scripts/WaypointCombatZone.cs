using UnityEngine;

public class WaypointCombatZone : MonoBehaviour
{
    [SerializeField] private EnemySpawnerWaypoint enemySpawner;
    [SerializeField] private GameObject marker;
    [SerializeField] private GameObject nextWaypoint;

    private bool started = false;
    private bool completed = false;

    private void Start()
    {
        if (enemySpawner != null)
            enemySpawner.StopWave();
    }

    private void Update()
    {
        if (!started || completed || enemySpawner == null)
            return;

        if (enemySpawner.WaveComplete)
        {
            completed = true;

            if (nextWaypoint != null)
                nextWaypoint.SetActive(true);

            if (marker != null)
                marker.SetActive(false);

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
            if (nextWaypoint == null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (started || completed)
            return;

        if (!other.CompareTag("Player"))
            return;

        started = true;

        if (marker != null)
            marker.SetActive(false);

        if (enemySpawner != null)
            enemySpawner.BeginWave();
        else
            Debug.LogError("WaypointCombatZone is missing EnemySpawnerWaypoint reference on " + gameObject.name);
    }
}