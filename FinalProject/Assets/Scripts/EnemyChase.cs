using System.Collections;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Rigidbody2D rb; 
    [SerializeField] private Transform[] startPoints;
    [SerializeField] private Transform pathContainer;

    [Header("Movement")]
    [SerializeField] private float obstacleDetectionRadius = 0.1f;
    [SerializeField] private float avoidanceForce = 100f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Behavior")]
    public float attackCooldown = 0.2f;
    public int towerDamageValue = 10;
    public int playerDamageValue = 5;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportDistance = 1.5f; // Distance to teleport forward
    [SerializeField] private float minTeleportAngle = 15f; // Minimum angle variation
    [SerializeField] private float maxTeleportAngle = 45f; // Maximum angle variation
    [SerializeField] private float stuckTimeThreshold = 2f; // Time before teleporting
    [SerializeField] private LayerMask wallLayer; // Layer for wall obstacles
    [SerializeField] private float downwardTeleportBoost = 0.5f;

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
    private EnemyStats statsSO;

    private bool isStuck = false;
    private float stuckTimer = 0f;
    private Vector2 lastPosition;
    private float positionCheckInterval = 0.5f;
    private float lastPositionCheckTime = 0f;

    void Start()
    {
   
        StartCoroutine(RecalculatePathRoutine());
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isStopped || (isChasing && chaseTarget == null)) return;

        if (!isChasing && path != null && pathIndex < path.Length && target != null)
        {
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
        CheckIfStuck();
    }
    public void SetStats(EnemyStats stats)
    {
        statsSO = stats;
    }

    void FixedUpdate()
    {
        if (isStopped)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = CalculateMovementDirection();
        direction = ApplyObstacleAvoidance(direction);
        rb.linearVelocity = direction * statsSO.moveSpeed; ;
    }

    private Vector2 CalculateMovementDirection()
    {
        if (isChasing && chaseTarget != null)
        {
            return (chaseTarget.position - transform.position).normalized;
        }
        else if (target != null)
        {
            return (target.position - transform.position).normalized;
        }
        else if (path != null && pathIndex < path.Length)
        {
            return (path[pathIndex].position - transform.position).normalized;
        }

        return Vector2.zero;
    }

    private Vector2 ApplyObstacleAvoidance(Vector2 movementDirection)
    {
        // Cast rays in 8 directions around the enemy
        Vector2 avoidanceForce = Vector2.zero;
        int rays = 8;
        float angleStep = 360f / rays;

        for (int i = 0; i < rays; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dir,
                obstacleDetectionRadius,
                obstacleLayer
            );

            // Visualize rays in editor
            Debug.DrawRay(
                transform.position,
                dir * obstacleDetectionRadius,
                hit.collider ? Color.red : Color.green
            );

            if (hit.collider != null)
            {
                // Calculate avoidance force based on hit distance
                float forceMagnitude = obstacleDetectionRadius - hit.distance;
                avoidanceForce += (Vector2)hit.normal * forceMagnitude;
            }
        }

        // Apply steering behavior
        if (avoidanceForce != Vector2.zero)
        {
            // Blend avoidance with movement direction
            Vector2 avoidanceDirection = avoidanceForce.normalized;
            return (movementDirection + avoidanceDirection * this.avoidanceForce).normalized;
        }

        return movementDirection;
    }

    private IEnumerator RecalculatePathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (isChasing || path == null || pathIndex >= path.Length) continue;

            // Find next visible waypoint
            int newIndex = pathIndex;
            for (int i = pathIndex; i < path.Length; i++)
            {
                Vector2 direction = path[i].position - transform.position;
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    direction.normalized,
                    direction.magnitude,
                    obstacleLayer
                );

                if (hit.collider == null)
                {
                    newIndex = i;
                    break;
                }
            }

            if (newIndex != pathIndex)
            {
                pathIndex = newIndex;
                target = path[pathIndex];
            }
        }
    }

    private void InitializePath()
    {
        if (pathContainer != null && pathContainer.childCount > 0)
        {
            path = new Transform[pathContainer.childCount];
            for (int i = 0; i < pathContainer.childCount; i++)
            {
                path[i] = pathContainer.GetChild(i);
            }
        }

        if (path != null && path.Length > 0)
        {
            // Choose random spawn point
            if (startPoints != null && startPoints.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, startPoints.Length);
                transform.position = startPoints[randomIndex].position;
            }
            else
            {
                transform.position = path[0].position;
            }

            // Find closest waypoint to spawn position
            pathIndex = FindClosestWaypoint();
            target = path[pathIndex];
        }
    }

    private int FindClosestWaypoint()
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < path.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, path[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
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
            pathIndex = FindClosestWaypoint();
            target = path[pathIndex];
        }
    }

    public void InitPath(Transform[] pathArray)
    {
        path = pathArray;
        pathIndex = 0;

        if (path != null && path.Length > 0)
        {
            // Find closest waypoint to current position
            pathIndex = FindClosestWaypoint();
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
    private void CheckIfStuck()
    {
        if (!isChasing || lockedInCombat || isStopped)
        {
            isStuck = false;
            stuckTimer = 0f;
            return;
        }

        if (Time.time > lastPositionCheckTime + positionCheckInterval)
        {
            lastPositionCheckTime = Time.time;

            float distanceMoved = Vector2.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            if (distanceMoved < 0.1f)
            {
                stuckTimer += positionCheckInterval;

                if (stuckTimer >= stuckTimeThreshold)
                {
                    TryTeleport();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
                isStuck = false;
            }
        }
    }
    private void TryTeleport()
    {
        Vector2 moveDirection = CalculateMovementDirection();

        // SPECIAL HANDLING FOR DOWNWARD MOVEMENT
        bool isMovingDown = moveDirection.y < -0.5f;
        float verticalBoost = isMovingDown ? downwardTeleportBoost : 0f;

        // Try multiple directions with emphasis on downward movement
        for (int i = 0; i < 8; i++)
        {
            Vector2 teleportDirection = GetTeleportDirection(moveDirection, isMovingDown);
            Vector2 teleportPosition = (Vector2)transform.position +
                                      teleportDirection * (teleportDistance + (isMovingDown ? verticalBoost : 0f));

            if (IsValidTeleportPosition(teleportPosition))
            {
                Teleport(teleportPosition);
                return;
            }
        }

        // Fallback: Try original direction with extra boost if moving down
        Vector2 fallbackPosition = (Vector2)transform.position +
                                  moveDirection * (teleportDistance + (isMovingDown ? verticalBoost * 2f : 0f));
        Teleport(fallbackPosition);
    }

    private Vector2 GetTeleportDirection(Vector2 baseDirection, bool isMovingDown)
    {
        // Add random angle variation
        float angle = Random.Range(minTeleportAngle, maxTeleportAngle);
        angle *= Random.Range(0, 2) == 0 ? 1 : -1;

        // Create rotated direction
        Vector2 rotatedDirection = Quaternion.Euler(0, 0, angle) * baseDirection;

        // For downward movement, prioritize maintaining downward momentum
        if (isMovingDown)
        {
            // Bias toward maintaining downward direction
            rotatedDirection.y = Mathf.Min(rotatedDirection.y, baseDirection.y);

            // Add extra downward component
            rotatedDirection.y -= Random.Range(0.1f, 0.3f);
            return rotatedDirection.normalized;
        }

        return rotatedDirection;
    }

    private bool IsValidTeleportPosition(Vector2 position)
    {
        // Check for walls with a circle cast
        Collider2D hit = Physics2D.OverlapCircle(position, 0.3f, wallLayer);

        // Additional check to prevent teleporting outside play area
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 viewportPos = mainCam.WorldToViewportPoint(position);
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            {
                return false;
            }
        }

        return hit == null;
    }

    private void Teleport(Vector2 position)
    {
        // Visual effect
        Debug.Log("Enemy teleporting to bypass obstacle!");

        // Actual teleport
        transform.position = position;

        // Reset path to prevent getting stuck again
        if (path != null && path.Length > 0)
        {
            pathIndex = FindClosestWaypoint();
            target = path[pathIndex];
        }
    }
}
