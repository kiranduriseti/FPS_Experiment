using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;   // orange, blue, green
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 3f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0f;

        Vector3 candidatePosition = transform.position + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePosition, out hit, 10f, NavMesh.AllAreas))
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);

            Instantiate(enemyPrefabs[randomIndex], hit.position, Quaternion.identity);
        }
    }
}