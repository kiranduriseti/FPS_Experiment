using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
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
    [SerializeField] private float spawnHeight = 0.10f;

    private float spawnTimer = 0f;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= refillCooldown)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        EnemyController[] meleeEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        EnemyRangedController[] rangedEnemies = FindObjectsByType<EnemyRangedController>(FindObjectsSortMode.None);

        int meleeCount = meleeEnemies.Length;
        int rangedCount = rangedEnemies.Length;

        bool canSpawnMelee = meleeCount < maxMeleeEnemies && meleeEnemyPrefabs.Length > 0;
        bool canSpawnRanged = rangedCount < maxRangedEnemies && rangedEnemyPrefabs.Length > 0;

        if (!canSpawnMelee && !canSpawnRanged)
            return;

        GameObject prefabToSpawn = null;

        if (canSpawnMelee && canSpawnRanged)
        {
            float meleeMissing = maxMeleeEnemies - meleeCount;
            float rangedMissing = maxRangedEnemies - rangedCount;
            float totalMissing = meleeMissing + rangedMissing;

            float roll = Random.value * totalMissing;

            if (roll < meleeMissing)
            {
                prefabToSpawn = meleeEnemyPrefabs[Random.Range(0, meleeEnemyPrefabs.Length)];
                spawnHeight = 0.2f;
            }
            else
            {
                prefabToSpawn = rangedEnemyPrefabs[Random.Range(0, rangedEnemyPrefabs.Length)];
                spawnHeight = 15f;
            }
        }
        else if (canSpawnMelee)
        {
            prefabToSpawn = meleeEnemyPrefabs[Random.Range(0, meleeEnemyPrefabs.Length)];
        }
        else if (canSpawnRanged)
        {
            prefabToSpawn = rangedEnemyPrefabs[Random.Range(0, rangedEnemyPrefabs.Length)];
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
                Debug.Log("Spawning prefab: " + prefabToSpawn.name);
                Instantiate(prefabToSpawn, hit.position + Vector3.up * 0.2f, Quaternion.identity);

                spawnTimer = 0f;   // reset cooldown only after successful spawn
                return;
            }
        }
    }
}