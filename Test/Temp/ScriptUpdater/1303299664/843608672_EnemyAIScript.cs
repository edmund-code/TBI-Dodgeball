using UnityEngine;
using UnityEngine.AI;

public class EnemyAIScript : MonoBehaviour 
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform floorsParent; // Reference to the Floors parent object

    [Header("Layers")]
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Stats")]
    public float health = 100f;

    [Header("Patrolling")]
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange = 10f;
    public float pathCenteringForce = 2f; // How strongly to stay on path

    [Header("Attacking")]
    public float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;
    public GameObject projectile;
    public float projectileForwardOffset = 1f;
    public float projectileSpeed = 32f;
    public float projectileArcHeight = 8f;

    [Header("Ranges")]
    public float sightRange = 15f;
    public float attackRange = 5f;
    private bool playerInSightRange, playerInAttackRange;

    [Header("Movement & Rotation")]
    public float chaseStoppingDistance = 2f;
    public float rotationSpeed = 10f;
    public float acceleration = 15f;

    private Vector3 predictedPlayerPosition;
    private float aimUpdateRate = 0.1f;
    private float nextAimUpdate;

    private void Start()
    {
        if (!player) 
        {
            player = GameObject.Find("PlayerObj").transform;
        }

        if (!floorsParent)
        {
            floorsParent = GameObject.Find("Floors").transform;
        }

        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = chaseStoppingDistance;
        agent.acceleration = acceleration;
        agent.updateRotation = false;
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Update aim prediction if player is in sight
        if (playerInSightRange && Time.time >= nextAimUpdate)
        {
            UpdateAimPrediction();
            nextAimUpdate = Time.time + aimUpdateRate;
        }

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInSightRange && playerInAttackRange)
        {
            AttackPlayer();
        }

        // Always try to stay centered on the path
        StayOnPath();

        // Rotate towards predicted position if player is in sight
        if (playerInSightRange)
        {
            RotateTowardsPredictedPosition();
        }
    }

    private void StayOnPath()
    {
        if (floorsParent != null)
        {
            // Find the nearest point on the center of the path
            Vector3 centerPoint = floorsParent.position;
            centerPoint.x = transform.position.x; // Keep current X position
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(centerPoint, out hit, 100f, NavMesh.AllAreas))
            {
                Vector3 centeringDirection = (hit.position - transform.position);
                centeringDirection.y = 0;

                // Only apply centering if too far from center
                if (centeringDirection.magnitude > 0.5f)
                {
                    Vector3 targetPosition = transform.position + centeringDirection.normalized * pathCenteringForce * Time.deltaTime;
                    agent.SetDestination(targetPosition);
                }
            }
        }
    }

    private void UpdateAimPrediction()
    {
        if (player != null)
        {
            // Get player's velocity if they have a Rigidbody
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            Vector3 playerVelocity = (playerRb != null) ? playerRb.linearVelocity : Vector3.zero;

            // Calculate time for projectile to reach player
            float distance = Vector3.Distance(transform.position, player.position);
            float timeToReach = distance / projectileSpeed;

            // Predict where the player will be
            predictedPlayerPosition = player.position + playerVelocity * timeToReach;
            
            // Adjust prediction based on ground height
            RaycastHit groundHit;
            if (Physics.Raycast(predictedPlayerPosition + Vector3.up * 10f, Vector3.down, out groundHit, 20f, whatIsGround))
            {
                predictedPlayerPosition.y = groundHit.point.y;
            }
        }
    }

    private void RotateTowardsPredictedPosition()
    {
        if (player == null) return;

        Vector3 targetPosition = playerInSightRange ? predictedPlayerPosition : player.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        if (!alreadyAttacked)
        {
            Vector3 spawnPos = transform.position + transform.forward * projectileForwardOffset;
            Rigidbody rb = Instantiate(projectile, spawnPos, transform.rotation).GetComponent<Rigidbody>();

            // Calculate direction to predicted position
            Vector3 directionToPredicted = (predictedPlayerPosition - spawnPos).normalized;
            
            // Apply forces for projectile trajectory
            rb.AddForce(directionToPredicted * projectileSpeed, ForceMode.Impulse);
            rb.AddForce(Vector3.up * projectileArcHeight, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // Keep existing methods unchanged
    private void Patroling() { /* Your existing code */ }
    private void SearchWalkPoint() { /* Your existing code */ }
    private void ChasePlayer() { /* Your existing code */ }
    private void ResetAttack() { /* Your existing code */ }
    private void TakeDamage(int damage) { /* Your existing code */ }
    private void DestroyEnemy() { /* Your existing code */ }

    private void OnDrawGizmosSelected()
    {
        // Original gizmos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw predicted aim position when in play mode
        if (Application.isPlaying && playerInSightRange)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, predictedPlayerPosition);
            Gizmos.DrawWireSphere(predictedPlayerPosition, 0.5f);
        }
    }
}