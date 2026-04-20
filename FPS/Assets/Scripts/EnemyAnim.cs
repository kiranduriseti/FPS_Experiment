using UnityEngine;

public class EnemyAnim : MonoBehaviour
{
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    public void Attack()
    {
        if (enemy != null)
        {
            enemy.Attack();
        }
    }
}