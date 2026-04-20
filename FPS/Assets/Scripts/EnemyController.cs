using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float patrolRadius = 20f;
    public float aggroRadius = 10f;
    public float attackRadius = 3f;
    public float health = 20f;
    public float minPatrolTime = 2f;
    public float maxPatrolTime = 5f;
    public float attackCooldownTime = 3f;
    public float contactDamage = 10f;

    [Header("Detection")]
    public LayerMask playerMask;

    private Animator anim;
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform target;
    private bool canAttack = true;
    private float maxHealth;
    public event System.Action<float, float> OnHealthChanged;

    [Header("Drops")]
    [SerializeField] private GameObject ammoPickupPrefab;
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float healthDropChance = 0.25f;

    private enum State
    {
        Patrol,
        Aggro,
        Dead
    }

    private State state;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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

        if (agent != null && agent.speed > 0f)
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
        }
    }

    private void HandleAggro()
    {
        if (target == null)
        {
            state = State.Patrol;
            StartCoroutine(ChoosePatrolLocation());
            return;
        }

        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null && player.health <= 0f)
        {
            state = State.Dead;
            return;
        }

        agent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRadius && canAttack)
        {
            anim.SetTrigger("Attack");
            StartCoroutine(AttackCooldown());
        }

        if (distance > aggroRadius * 1.5f)
        {
            target = null;
            state = State.Patrol;
            StartCoroutine(ChoosePatrolLocation());
        }
    }

    public void Attack()
    {
        if (state == State.Dead)
            return;

        Vector3 attackCenter = transform.position + transform.forward * attackRadius;

        foreach (Collider col in Physics.OverlapSphere(attackCenter, attackRadius, playerMask))
        {
            if (col.CompareTag("Player"))
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.playerhit(contactDamage);
                }
            }
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

        if (agent != null)
        {
            agent.isStopped = true;
        }

        anim.SetTrigger("Dead");

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
            agent.SetDestination(destination);

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
                if (player != null && player.health > 0f)
                {
                    target = col.transform;
                    return true;
                }
            }
        }

        return false;
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