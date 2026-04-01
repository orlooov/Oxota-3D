using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class WolfAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;
    public float rotationSpeed = 120f;
    public float attackDistance = 1.5f;
    public float detectionRange = 10f;
    public float patrolRange = 5f;
    public float attackDuration = 1.5f;
    public float walkToRunThreshold = 3f;

    [Header("Combat Settings")]
    public int maxHealth = 30;
    public int attackDamage = 15;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Effects")]
    public ParticleSystem bloodEffect;
    public GameObject attackHitBox;

    [Header("Despawn Settings")]
    public float despawnDelay = 60f;

    [Header("Quest")]
    public string animalType = "Wolf"; // Тип животного для квеста

    // Компоненты
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private int currentHealth;

    // Состояния
    private enum WolfState { Idle, Patrolling, Chasing, Attacking, Hit, Dead }
    private WolfState currentState;
    private Vector3 patrolCenter;
    private float attackTimer;
    private float deathTimer;

    private Health playerHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<Health>();

        currentHealth = maxHealth;
        patrolCenter = transform.position;

        InitializeAgent();
        SetRandomPatrolPoint();

        if (attackHitBox != null)
            attackHitBox.SetActive(false);

        agent.acceleration = 8;
        Application.targetFrameRate = 60;
    }

    void InitializeAgent()
    {
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    void FixedUpdate()
    {
        if (currentState == WolfState.Dead)
        {
            UpdateDeathState();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case WolfState.Idle:
                UpdateIdleState(distanceToPlayer);
                break;

            case WolfState.Patrolling:
                UpdatePatrolState(distanceToPlayer);
                break;

            case WolfState.Chasing:
                UpdateChaseState(distanceToPlayer);
                break;

            case WolfState.Attacking:
                UpdateAttackState(distanceToPlayer);
                break;

            case WolfState.Hit:
                break;
        }
    }

    void Update()
    {
        UpdateAnimatorParameters();
    }

    void UpdateIdleState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentState = WolfState.Chasing;
        }
        else if (ShouldPatrol())
        {
            currentState = WolfState.Patrolling;
            SetRandomPatrolPoint();
        }
    }

    void UpdatePatrolState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentState = WolfState.Chasing;
        }
        else if (agent.remainingDistance < 0.5f)
        {
            SetRandomPatrolPoint();
        }
    }

    void UpdateChaseState(float distanceToPlayer)
    {
        bool shouldRun = distanceToPlayer > walkToRunThreshold;
        agent.speed = shouldRun ? runSpeed : walkSpeed;
        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackDistance)
        {
            StartAttack();
        }
    }

    void UpdateAttackState(float distanceToPlayer)
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            EndAttack();
        }
        else
        {
            FaceTarget(player.position);
        }
    }

    void StartAttack()
    {
        currentState = WolfState.Attacking;
        attackTimer = attackDuration;
        agent.isStopped = true;
        animator.SetTrigger("Attack");

        if (attackHitBox != null)
            attackHitBox.SetActive(true);
    }

    void EndAttack()
    {
        currentState = WolfState.Chasing;
        agent.isStopped = false;
        if (attackHitBox != null)
            attackHitBox.SetActive(false);
    }

    void UpdateAnimatorParameters()
    {
        bool isRunning = agent.speed == runSpeed && agent.velocity.magnitude > 0.1f;
        animator.SetBool("Run", isRunning);
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    void SetRandomPatrolPoint()
    {
        Vector3 randomPoint = patrolCenter + UnityEngine.Random.insideUnitSphere * patrolRange;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    public void OnAttackHit()
    {
        if (attackHitBox != null && attackHitBox.activeSelf)
        {
            if (Vector3.Distance(transform.position, player.position) <= attackDistance * 1.2f)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void OnAttackEnd()
    {
        animator.ResetTrigger("Attack");
        EndAttack();
    }

    public void TakeDamage(int damage, Vector3 hitPoint)
    {
        if (currentState == WolfState.Dead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (bloodEffect != null)
            Instantiate(bloodEffect, hitPoint, Quaternion.identity);

        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = WolfState.Chasing;
            detectionRange *= 1.5f;
        }
    }

    void Die()
    {
        // Засчитываем убийство волка ПЕРЕД смертью (всегда)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportKill();
        }

        currentState = WolfState.Dead;
        agent.isStopped = true;
        animator.SetTrigger("Death");

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        deathTimer = despawnDelay;
    }

    void UpdateDeathState()
    {
        deathTimer -= Time.deltaTime;

        if (deathTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    bool ShouldPatrol()
    {
        return true;
    }
}