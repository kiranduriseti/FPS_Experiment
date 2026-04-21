using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyRangedController : MonoBehaviour
{
    [Header("Stats")]
    public float patrolRadius = 20f;
    public float aggroRadius = 15f;
    public float attackRadius = 20f;
    public float health = 20f;
    public float minPatrolTime = 2f;
    public float maxPatrolTime = 5f;
    public float attackCooldownTime = 2.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileFireDelay = 0.35f;
    [SerializeField] private float aimHeight = 1.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask playerMask;

    [Header("Drops")]
    [SerializeField] private GameObject ammoPickupPrefab;
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float healthDropChance = 0.25f;

    private Animator anim;
    private NavMeshAgent agent;
    private Transform target;
    private bool canAttack = true;
    private float maxHealth;

    public event System.Action<float, float> OnHealthChanged;

    private enum State
    {
        Patrol,
        Aggro,
        Attack,
        Dead
    }

    private State state;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        maxHealth = health;
        state = State.Patrol;
        StartCoroutine(ChoosePatrolLocation());
    }

    private void Update()
    {
        if (state == State.Dead)
            return;

        if (agent != null && anim != null && agent.speed > 0f)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
        }

        switch (state)
        {
            case State.Patrol:
                if (CheckAggro())
                {
                    state = State.Aggro;
                }
                break;

            case State.Aggro:
                HandleAggro();
                break;

            case State.Attack:
                HandleAttack();
                break;
        }
    }

    private void HandleAggro()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        PlayerController player = target.GetComponent<PlayerController>();
        if (player == null)
            player = target.GetComponentInParent<PlayerController>();

        if (player != null && player.health <= 0f)
        {
            state = State.Dead;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > aggroRadius * 1.5f)
        {
            target = null;
            ReturnToPatrol();
            return;
        }

        FaceTarget();

        if (distance <= attackRadius)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            if (canAttack)
            {
                state = State.Attack;
            }

            return;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    private void HandleAttack()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRadius)
        {
            state = State.Aggro;
            return;
        }

        FaceTarget();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (canAttack)
        {
            if (anim != null)
                anim.SetTrigger("Projectile Attack 01");

            StartCoroutine(FireProjectileAfterDelay(projectileFireDelay));
            StartCoroutine(AttackCooldown());
        }

        state = State.Aggro;
    }

    private IEnumerator FireProjectileAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (state != State.Dead)
        {
            FireProjectile();
        }
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }
    }

    public void FireProjectile()
    {
        if (state == State.Dead)
            return;

        if (projectilePrefab == null || projectileSpawnPoint == null || target == null)
            return;

        Vector3 aimPoint = target.position + Vector3.up * aimHeight;
        Vector3 dir = (aimPoint - projectileSpawnPoint.position).normalized;

        GameObject proj = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(dir)
        );

        EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(dir, projectileDamage);
        }

        Collider myCol = GetComponent<Collider>();
        Collider projCol = proj.GetComponent<Collider>();

        if (myCol != null && projCol != null)
        {
            Physics.IgnoreCollision(projCol, myCol);
        }
    }

    public void enemyhit(float damage = 10f)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Dead)
            return;

        health -= damage;
        health = Mathf.Max(health, 0f);

        OnHealthChanged?.Invoke(maxHealth, health);

        if (health <= 0f)
        {
            Die();

            if (ammoPickupPrefab != null)
            {
                Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position;
                Instantiate(ammoPickupPrefab, spawnPos, Quaternion.identity);
            }

            if (healthPickupPrefab != null && Random.value <= healthDropChance)
            {
                Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position;
                Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity);
            }

            Destroy(gameObject, 3f);
        }
    }

    private void Die()
    {
        state = State.Dead;
        canAttack = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
    }

    private IEnumerator ChoosePatrolLocation()
    {
        while (state == State.Patrol)
        {
            Vector3 offset = Random.insideUnitSphere * patrolRadius;
            offset.y = 0f;

            Vector3 destination = transform.position + offset;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
            }

            yield return new WaitForSeconds(Random.Range(minPatrolTime, maxPatrolTime));
        }
    }

    private bool CheckAggro()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, aggroRadius, playerMask);

        foreach (Collider col in cols)
        {
            if (col.CompareTag("Player"))
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player == null)
                    player = col.GetComponentInParent<PlayerController>();

                if (player != null && player.health > 0f)
                {
                    target = player.transform;
                    return true;
                }
            }
        }

        return false;
    }

    private void ReturnToPatrol()
    {
        state = State.Patrol;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        StartCoroutine(ChoosePatrolLocation());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state == State.Dead)
            return;

        if (other.CompareTag("Spell"))
        {
            enemyhit();
        }
    }
}