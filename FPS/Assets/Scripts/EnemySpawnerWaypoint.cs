using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySpawnerWaypoint : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] meleeEnemyPrefabs;
    [SerializeField] private GameObject[] rangedEnemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float refillCooldown = 1f;
    [SerializeField] private int maxMeleeEnemies = 5;
    [SerializeField] private int maxRangedEnemies = 2;
    [SerializeField] private float navMeshSampleDistance = 10f;
    [SerializeField] private float meleeSpawnHeight = 0.2f;
    [SerializeField] private float rangedSpawnHeight = 15f;

    [Header("Wave Settings")]
    [SerializeField] private int killsRequired = 20;

    private float spawnTimer = 0f;

    private bool waveActive = false;
    private bool waveFinishedSpawning = false;
    private int enemiesSpawnedThisWave = 0;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    public bool WaveComplete { get; private set; } = false;

    private void Update()
    {
        if (!waveActive || WaveComplete)
            return;

        CleanupDeadEnemies();

        if (!waveFinishedSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= refillCooldown)
            {
                TrySpawnEnemy();
            }
        }

        CheckWaveComplete();
    }

    public void BeginWave()
    {
        waveActive = true;
        waveFinishedSpawning = false;
        WaveComplete = false;
        enemiesSpawnedThisWave = 0;
        spawnTimer = 0f;
        spawnedEnemies.Clear();
    }

    public void StopWave()
    {
        waveActive = false;
    }

    private void TrySpawnEnemy()
    {
        if (enemiesSpawnedThisWave >= killsRequired)
        {
            waveFinishedSpawning = true;
            return;
        }

        int meleeCount = 0;
        int rangedCount = 0;

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] == null)
                continue;

            if (spawnedEnemies[i].GetComponent<EnemyController>() != null)
                meleeCount++;
            else if (spawnedEnemies[i].GetComponent<EnemyRangedController>() != null)
                rangedCount++;
        }

        bool canSpawnMelee = meleeCount < maxMeleeEnemies && meleeEnemyPrefabs.Length > 0;
        bool canSpawnRanged = rangedCount < maxRangedEnemies && rangedEnemyPrefabs.Length > 0;

        if (!canSpawnMelee && !canSpawnRanged)
            return;

        GameObject prefabToSpawn = null;
        float spawnHeightToUse = meleeSpawnHeight;

        if (canSpawnMelee && canSpawnRanged)
        {
            float meleeMissing = maxMeleeEnemies - meleeCount;
            float rangedMissing = maxRangedEnemies - rangedCount;
            float totalMissing = meleeMissing + rangedMissing;

            float roll = Random.value * totalMissing;

            if (roll < meleeMissing)
            {
                prefabToSpawn = meleeEnemyPrefabs[Random.Range(0, meleeEnemyPrefabs.Length)];
                spawnHeightToUse = meleeSpawnHeight;
            }
            else
            {
                prefabToSpawn = rangedEnemyPrefabs[Random.Range(0, rangedEnemyPrefabs.Length)];
                spawnHeightToUse = rangedSpawnHeight;
            }
        }
        else if (canSpawnMelee)
        {
            prefabToSpawn = meleeEnemyPrefabs[Random.Range(0, meleeEnemyPrefabs.Length)];
            spawnHeightToUse = meleeSpawnHeight;
        }
        else if (canSpawnRanged)
        {
            prefabToSpawn = rangedEnemyPrefabs[Random.Range(0, rangedEnemyPrefabs.Length)];
            spawnHeightToUse = rangedSpawnHeight;
        }

        if (prefabToSpawn == null)
            return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0f;

            Vector3 candidatePosition = transform.position + randomOffset;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                GameObject spawned = Instantiate(
                    prefabToSpawn,
                    hit.position + Vector3.up * spawnHeightToUse,
                    Quaternion.identity
                );

                spawnedEnemies.Add(spawned);
                enemiesSpawnedThisWave++;

                if (enemiesSpawnedThisWave >= killsRequired)
                {
                    waveFinishedSpawning = true;
                }

                spawnTimer = 0f;
                return;
            }
        }
    }

    private void CleanupDeadEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private void CheckWaveComplete()
    {
        if (waveFinishedSpawning && spawnedEnemies.Count == 0)
        {
            WaveComplete = true;
            waveActive = false;
        }
    }
}