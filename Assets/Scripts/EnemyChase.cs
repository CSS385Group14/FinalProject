using System.Collections;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject[] startPoints; // Random spawn points

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    public float attackCooldown = 0.2f;
    public int towerDamageValue = 10;
    public int playerDamageValue = 5;

    private Transform[] path;
    private int pathIndex = 0;
    private Transform target;
    private Vector2 currentDestination;

    private Transform chaseTarget;
    private bool isChasing = false;
    private bool stopRequestedExternally = false;
    private bool stoppedByBarricade = false;

    public bool lockedInCombat { get; set; } = false;
    private bool isStopped => stopRequestedExternally || stoppedByBarricade;

    void Start()
    {
        StartCoroutine(RecalculatePathRoutine());
    }

    void Update()
    {
        if (isChasing || isStopped || path == null || target == null)
            return;

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
        if (isStopped)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = Vector2.zero;

        if (isChasing && chaseTarget != null)
        {
            direction = (chaseTarget.position - transform.position).normalized;
        }
        else if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }

        direction = ObstacleAvoidance(direction);
        rb.linearVelocity = direction * moveSpeed;
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

                foreach (var testDir in testDirs)
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

    private IEnumerator RecalculatePathRoutine()
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
                pathIndex--;
        }
    }

    public void InitPath(Transform[] pathArray)
    {
        path = pathArray;
        pathIndex = 0;

        if (path.Length > 0)
        {
            // Choose random start position
            if (startPoints != null && startPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, startPoints.Length);
                transform.position = startPoints[randomIndex].transform.position;

            }
            else
            {
                transform.position = path[0].position;
            }

            currentDestination = path[path.Length - 1].position;
            pathIndex = 1;
            if (path.Length > 1)
                target = path[pathIndex];
        }
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

        if (path != null && path.Length > 0)
        {
            GameObject tempTarget = new GameObject("TempTarget");
            tempTarget.transform.position = currentDestination;
            target = tempTarget.transform;
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

        if (path != null && pathIndex < path.Length)
        {
            target = path[pathIndex];
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
