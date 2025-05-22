using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    public float attackCooldown = 0.2f;
    public int towerDamageValue = 10;
    public int playerDamageValue = 5;

    // Chase system additions
    private Transform chaseTarget;
    private bool isChasing = false;

    private Transform target;
    private float nextFireTime = 0f;
    private Transform[] path;
    private int pathIndex = 0;
    private MainTower mainTower;
    private bool stopRequestedExternally = false;
    private bool stoppedByBarricade = false;

    public bool lockedInCombat { get; set; } = false;
    private bool isStopped => stopRequestedExternally || stoppedByBarricade;
    private Vector2 currentDestination;
    void Start()
    {
        StartCoroutine(RecalculatePath());
    }

    void Update()
    {
        if (isChasing) return;

        if (path == null || target == null || isStopped) return;

        if (Vector2.Distance(transform.position, target.position) <= 0.1f)
        {
            pathIndex++;
            if (pathIndex >= path.Length)
            {
                EnemySpawner.onEnemyDestroy?.Invoke();
                Destroy(gameObject);
                return;
            }
            target = path[pathIndex];
        }
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            if (chaseTarget != null && !isStopped)
            {
                Vector2 direction = (chaseTarget.position - transform.position).normalized;
                direction = ObstacleAvoidance(direction);
                rb.velocity = direction * moveSpeed;
            }
            return;
        }

        if (target == null || path == null) return;

        if (!isStopped)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            direction = ObstacleAvoidance(direction);
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private IEnumerator RecalculatePath()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (isChasing || path == null || pathIndex >= path.Length) continue;

            RaycastHit2D hit = Physics2D.Linecast(
                transform.position,
                path[pathIndex].position,
                LayerMask.GetMask("Obstacle")
            );

            if (hit.collider != null && pathIndex > 1)
            {
                pathIndex--;
            }
        }
    }

    private Vector2 ObstacleAvoidance(Vector2 currentDirection)
    {
        float rayDistance = 1.5f;
        float avoidStrength = 8f;
        int rays = 3;
        Vector2 bestDirection = currentDirection;

        for (int i = 0; i < rays; i++)
        {
            float angle = (i - (rays - 1) / 2f) * 30f;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * currentDirection;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dir,
                rayDistance,
                LayerMask.GetMask("Obstacle")
            );

            if (hit.collider != null)
            {
                Vector2 avoidDir = Vector2.Perpendicular(hit.normal).normalized;
                Vector2[] testDirs = { avoidDir, -avoidDir };

                foreach (Vector2 testDir in testDirs)
                {
                    if (!Physics2D.Raycast(transform.position, testDir, rayDistance, LayerMask.GetMask("Obstacle")))
                    {
                        bestDirection = Vector2.Lerp(currentDirection, testDir, Time.fixedDeltaTime * avoidStrength).normalized;
                        return bestDirection;
                    }
                }
            }
        }
        return bestDirection;
    }

    public void ChaseTarget(Transform target)
    {
        chaseTarget = target;
        isChasing = true;
    }

    public void ResumeWaypoints()
    {
        isChasing = false;
        chaseTarget = null;

        // Instead of returning to waypoints, continue toward final destination
        if (path != null && path.Length > 0)
        {
            // Create temporary target at current destination
            GameObject tempTarget = new GameObject("TempTarget");
            tempTarget.transform.position = currentDestination;
            target = tempTarget.transform;

            // Clean up temporary target when we reach it
            StartCoroutine(CleanupTempTarget(tempTarget));
        }
    }

    private IEnumerator CleanupTempTarget(GameObject targetObj)
    {
        while (Vector2.Distance(transform.position, targetObj.transform.position) > 0.1f)
        {
            yield return null;
        }
        Destroy(targetObj);

        // If we somehow didn't reach the real destination, fallback to original path
        if (path != null && pathIndex < path.Length)
        {
            target = path[pathIndex];
        }
    }

    // Modified path initialization
    public void InitPath(Transform[] pathArray)
    {
        path = pathArray;
        pathIndex = 0;
        if (path.Length > 0)
        {
            transform.position = path[0].position;
            currentDestination = path[path.Length - 1].position;
            target = path[1];
            pathIndex = 1;
        }
    }

    public void StopMovement(bool stop)
    {
        stopRequestedExternally = stop;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            stoppedByBarricade = true;
            StartCoroutine(AttackBarricade(collision.GetComponent<Barricade>()));
        }
    }

    private IEnumerator AttackBarricade(Barricade barricade)
    {
        while (barricade != null && barricade.health > 0)
        {
            barricade.TakeDamage(2);
            yield return new WaitForSeconds(attackCooldown);
        }
        stoppedByBarricade = false;
    }
}