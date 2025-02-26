using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AnimalAI : MonoBehaviour
{
    public enum BehaviorType { Approach, Flee }
    public BehaviorType behavior;

    public bool hides;
    public bool stayAtHideSpot;
    public string hideTag = "Bush";
    public float detectionRange = 10f;
    public float chaseThreshold = 15f; // Distance at which AI stops chasing
    public float attackRange = 2f;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    public float fleeDistance = 15f;
    public float walkSpeed = 3.5f;
    public float health = 100f;
    private bool isAttacking = false;

    private float runSpeed;
    private Transform player;
    private NavMeshAgent agent;
    private bool isHiding = false;
    private GameObject lastHidingSpot = null;
    private Vector3 lastWanderPosition;
    private bool isChasing = false;

    // Attack cooldown
    private float attackCooldown = 1f; // Time in seconds between attacks
    private float lastAttackTime = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        runSpeed = walkSpeed * 1.5f;

        if (stayAtHideSpot)
        {
            Hide();
        }
        else
        {
            StartCoroutine(Wander());
        }
        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isChasing && distanceToPlayer > chaseThreshold)
        {
            isChasing = false;
            agent.speed = walkSpeed;
            if (!stayAtHideSpot)
            {
                StartCoroutine(Wander());
            }
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            StopAllCoroutines();
            isChasing = true;
            agent.speed = runSpeed;
            if (hides)
            {
                Hide();
            }
            else
            {
                MoveTowardsOrAway(distanceToPlayer);
            }
        }
        else if (!isHiding && !stayAtHideSpot && !agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            StartCoroutine(Wander());
        }
        print(health);
    }

    void MoveTowardsOrAway(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            // Only allow attacking if enough time has passed
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time; // Update the time of the last attack
            }
        }
        else
        {
            Vector3 direction = (behavior == BehaviorType.Approach) ? player.position : transform.position * 2 - player.position;
            agent.SetDestination(direction);
        }
    }

    void Hide()
    {
        GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag(hideTag);
        GameObject nearestHidingSpot = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject spot in hidingSpots)
        {
            if (spot == lastHidingSpot) continue;

            float dist = Vector3.Distance(transform.position, spot.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestHidingSpot = spot;
            }
        }

        if (nearestHidingSpot != null)
        {
            agent.SetDestination(nearestHidingSpot.transform.position);
            lastHidingSpot = nearestHidingSpot;
            isHiding = true;
        }
    }

    void Attack()
    {
        Debug.Log(gameObject.name + " is attacking!");
    }

    IEnumerator Wander()
    {
        while (true)
        {
            yield return new WaitForSeconds(wanderTimer);

            Vector3 randomDirection;
            NavMeshHit navHit;

            do
            {
                randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += transform.position;
            } while (Vector3.Distance(randomDirection, lastWanderPosition) < 3f);

            if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1))
            {
                lastWanderPosition = navHit.position;
                agent.SetDestination(navHit.position);
            }

            yield return new WaitUntil(() => agent.remainingDistance <= 0.5f);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle the animal's death (e.g., destroy the object or play an animation)
        Destroy(gameObject);  // Example of destroying the animal on death.
    }
}