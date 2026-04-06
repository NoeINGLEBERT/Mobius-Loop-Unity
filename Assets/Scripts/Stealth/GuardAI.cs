using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GuardAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    public float patrolRadius = 15f;
    public float sightDistance = 12f;
    public float sightAngle = 70f;
    public float attackRange = 2f;
    public float loseSightTime = 5f;

    private float lostTimer;

    enum State { Patrol, Chase, Attack, Search }
    private State currentState;

    public float detectionRadius = 15f;
    public LayerMask playerLayer;

    private Transform targetPlayer; // current detected player

    [Header("Fail State")]
    public GameObject spottedWidgetPrefab; // UI prefab
    public float levelLoadDelay = 3f;
    public string nextSceneName = "GameOver";

    private bool gameOverTriggered = false;

    private float attackTimer = 0f;
    public float attackDelay = 1.5f; // delay before OnAttackHit
    private bool attackTriggered = false;

    public void OnAttackHit()
    {
        if (gameOverTriggered) return;
        if (targetPlayer == null) return;

        gameOverTriggered = true;

        TriggerGameOver();
    }

    void TriggerGameOver()
    {
        agent.isStopped = true;

        // Stop AI logic
        enabled = false;

        // Spawn "You've been spotted" UI
        if (spottedWidgetPrefab != null)
            Instantiate(spottedWidgetPrefab);

        // Load next scene after delay
        Invoke(nameof(LoadNextScene), levelLoadDelay);
    }


    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        GoToRandomPoint();
        currentState = State.Patrol;
    }

    void Update()
    {
        FindPlayer();

        bool seesPlayer = targetPlayer != null;
        float distance = seesPlayer ?
            Vector3.Distance(transform.position, targetPlayer.position) : Mathf.Infinity;

        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate(seesPlayer);
                break;

            case State.Chase:
                ChaseUpdate(seesPlayer, distance);
                break;

            case State.Attack:
                AttackUpdate(distance);
                break;

            case State.Search:
                SearchUpdate();
                break;
        }
    }

    // ================= PATROL =================
    void PatrolUpdate(bool seesPlayer)
    {
        anim.SetBool("isWalking", true);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToRandomPoint();

        if (seesPlayer)
        {
            currentState = State.Chase;
        }
    }

    void GoToRandomPoint()
    {
        Vector3 randomPos = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, patrolRadius, 1);
        agent.SetDestination(hit.position);
    }

    // ================= CHASE =================
    void ChaseUpdate(bool seesPlayer, float distance)
    {
        anim.SetBool("isWalking", true);
        anim.SetBool("isSearching", false);

        agent.SetDestination(targetPlayer.position);

        if (distance <= attackRange)
        {
            currentState = State.Attack;
            return;
        }

        if (!seesPlayer)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= loseSightTime)
            {
                currentState = State.Search;
                StartSearch();
            }
        }
        else
        {
            lostTimer = 0;
        }
    }

    // ================= ATTACK =================
    void AttackUpdate(float distance)
    {
        agent.isStopped = true;
        anim.SetBool("isAttacking", true);

        transform.LookAt(targetPlayer);

        // Only trigger attack after delay
        if (!attackTriggered)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDelay)
            {
                attackTriggered = true;
                OnAttackHit();
            }
        }

        // If player moves out of range, go back to chase
        if (distance > attackRange)
        {
            agent.isStopped = false;
            anim.SetBool("isAttacking", false);
            currentState = State.Chase;
        }
    }

    // ================= SEARCH =================
    void StartSearch()
    {
        agent.isStopped = true;
        anim.SetBool("isWalking", false);
        anim.SetBool("isSearching", true);

        Invoke(nameof(EndSearch), 3f); // search animation duration
    }

    void SearchUpdate() { }

    void EndSearch()
    {
        anim.SetBool("isSearching", false);
        agent.isStopped = false;
        GoToRandomPoint();
        currentState = State.Patrol;
        lostTimer = 0;
    }

    // ================= VISION =================
    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (targetPlayer.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, targetPlayer.position) > sightDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > sightAngle / 2)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out hit, sightDistance))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }

        return false;
    }

    void FindPlayer()
    {
        // If we already have a target, verify we still see it
        if (targetPlayer != null)
        {
            if (!CanSeeTarget(targetPlayer))
                targetPlayer = null;

            return;
        }

        // Scan nearby colliders in detection radius
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            playerLayer
        );

        foreach (Collider col in hits)
        {
            if (CanSeeTarget(col.transform))
            {
                targetPlayer = col.transform;
                lostTimer = 0;
                return;
            }
        }
    }

    bool CanSeeTarget(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > sightDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToTarget);
        if (angle > sightAngle / 2)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, dirToTarget, out hit, sightDistance))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }

        return false;
    }
}