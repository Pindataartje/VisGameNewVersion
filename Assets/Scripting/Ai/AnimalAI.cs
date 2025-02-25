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
    public float attackRange = 2f;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    public float fleeDistance = 15f;
    public float walkSpeed = 3.5f; // Walking speed (default speed)

    private float runSpeed; // Running speed (1.5 times the walking speed)
    private Transform player;
    private NavMeshAgent agent;
    private bool isHiding = false;
    private GameObject lastHidingSpot = null;
    private Vector3 lastWanderPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        // Initialize runSpeed as 1.5x of walkSpeed
        runSpeed = walkSpeed * 1.5f;

        if (stayAtHideSpot)
        {
            Hide();
        }
        else
        {
            StartCoroutine(Wander());
        }

        // Set the initial walking speed
        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Adjust the agent speed based on the current behavior
        if (behavior == BehaviorType.Flee || behavior == BehaviorType.Approach)
        {
            agent.speed = runSpeed; // Increase speed to runSpeed (1.5x of walkSpeed)
        }
        else
        {
            agent.speed = walkSpeed; // Use walkSpeed in other cases
        }

        if (isHiding && distanceToPlayer <= detectionRange && behavior == BehaviorType.Flee)
        {
            FindNewHidingSpotOrFlee();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            StopAllCoroutines();
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
    }

    void MoveTowardsOrAway(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            Attack();
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
            if (spot == lastHidingSpot) continue; // Avoid going back to the last hiding spot

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

    void FindNewHidingSpotOrFlee()
    {
        GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag(hideTag);
        GameObject bestHidingSpot = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject spot in hidingSpots)
        {
            if (spot == lastHidingSpot) continue; // Avoid returning to the last hiding spot

            float dist = Vector3.Distance(transform.position, spot.transform.position);
            if (dist > attackRange && dist < minDistance)
            {
                minDistance = dist;
                bestHidingSpot = spot;
            }
        }

        if (bestHidingSpot != null)
        {
            agent.SetDestination(bestHidingSpot.transform.position);
            lastHidingSpot = bestHidingSpot;
        }
        else
        {
            Flee();
        }
    }

    void Flee()
    {
        Vector3 fleeDirection = transform.position + (transform.position - player.position).normalized * fleeDistance;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(fleeDirection, out navHit, fleeDistance, -1))
        {
            agent.SetDestination(navHit.position);
        }
    }

    void Attack()
    {
        Debug.Log(gameObject.name + " is attacking!");
            print(" Attack");
    }

    IEnumerator Wander()
    {
        while (true)
        {
            // Wait for the specified wanderTimer before setting a new destination
            yield return new WaitForSeconds(wanderTimer);

            Vector3 randomDirection;
            NavMeshHit navHit;

            // Ensure the new random position is at least 3 meters away from the last position
            do
            {
                // Generate a random direction within the wander radius
                randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += transform.position;

            } while (Vector3.Distance(randomDirection, lastWanderPosition) < 3f);

            // Sample the NavMesh and set the destination if it's valid
            if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1))
            {
                lastWanderPosition = navHit.position;
                agent.SetDestination(navHit.position);
            }

            // Wait until the agent reaches its destination
            yield return new WaitUntil(() => agent.remainingDistance <= 0.5f);
        }
    }
}
