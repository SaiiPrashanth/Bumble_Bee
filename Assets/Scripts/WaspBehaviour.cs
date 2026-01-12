using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaspBehaviour : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    [SerializeField] private Transform model;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 15f;
    [SerializeField] private float chaseSpeed = 25f;
    [SerializeField] private float rotationSpeed = 180f;
    
    [Header("Behavior")]
    [SerializeField] private float aggroRange = 50f;
    [SerializeField] private float attackRange = 3f;
    
    // Properties set by Spawner
    [HideInInspector] public Vector3 spawnPoint;
    [HideInInspector] public float roamRadius = 100f;

    private Rigidbody rb;
    private bool isChasing;
    private Vector3 roamTarget;
    private float roamTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupRigidbody();
        
        // Ensure collider is non-trigger
        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = false;
        }

        // Initialize roaming
        spawnPoint = transform.position;
        PickNewRoamTarget();
        
        // Ensure model faces forward if needed (legacy fix)
        if (model != null)
        {
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    void SetupRigidbody()
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Roam();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= aggroRange)
        {
            Chase(distanceToTarget);
        }
        else
        {
            if (isChasing)
            {
                // Lost target
                isChasing = false;
                PickNewRoamTarget();
            }
            Roam();
        }
    }

    void Chase(float distance)
    {
        isChasing = true;
        
        // Determine speed based on distance
        float currentSpeed = (distance <= attackRange) ? chaseSpeed : patrolSpeed;

        // Move towards target
        Vector3 direction = (target.position - transform.position).normalized;
        MoveInDirection(direction, currentSpeed);
    }

    void Roam()
    {
        isChasing = false;
        
        Vector3 directionToTarget = roamTarget - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Check if reached destination or timer expired
        roamTimer -= Time.fixedDeltaTime;
        if (distanceToTarget < 5f || roamTimer <= 0f)
        {
            PickNewRoamTarget();
            return;
        }

        MoveInDirection(directionToTarget.normalized, patrolSpeed * 0.7f);
    }

    void MoveInDirection(Vector3 direction, float speed)
    {
        if (direction == Vector3.zero) return;

        // Rotate to face direction
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);

        // Move forward relative to self
        rb.linearVelocity = transform.forward * speed;
    }

    void PickNewRoamTarget()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-roamRadius, roamRadius),
            Random.Range(-20f, 20f),
            Random.Range(-roamRadius, roamRadius)
        );
        
        roamTarget = spawnPoint + randomOffset;
        roamTarget.y = Mathf.Max(roamTarget.y, 10f); // Keep above ground

        roamTimer = Random.Range(5f, 10f);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    void HandleCollision(GameObject other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bee"))
        {
            BeeBehaviour bee = other.GetComponent<BeeBehaviour>();
            if (bee == null) bee = other.GetComponentInParent<BeeBehaviour>();
            
            if (bee != null)
            {
                bee.TriggerGameOver(transform);
                Destroy(bee.gameObject);
                
                // Resume roaming
                isChasing = false;
                PickNewRoamTarget();
            }
        }
        
        // Destroy self if hit by a missile/projectile (if applicable)
        if (other.CompareTag("Missile"))
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPoint, roamRadius);
        
        if (isChasing && target != null)
        {
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
