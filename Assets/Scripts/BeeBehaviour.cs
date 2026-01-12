using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BeeBehaviour : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private bool invertForward = true;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float currentVerticalSpeed = 0f;
    private bool isGameOver;
    
    // Animator parameter hashes
    private int speedHash;
    private int verticalHash;
    private bool hasAnimator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupRigidbody();
        SetupColliders();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        if (animator != null)
        {
            hasAnimator = true;
            speedHash = Animator.StringToHash("Speed");
            verticalHash = Animator.StringToHash("Vertical");
        }
    }

    void SetupRigidbody()
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void SetupColliders()
    {
        // Ensure main physics collider
        if (!TryGetComponent<Collider>(out var mainCol))
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 1f;
            sphere.isTrigger = false;
        }
        else
        {
            mainCol.isTrigger = false;
        }

        // Ensure child trigger for flower collection
        Transform collector = transform.Find("FlowerCollector");
        if (collector == null)
        {
            GameObject colObj = new GameObject("FlowerCollector");
            colObj.transform.SetParent(transform, false);
            colObj.layer = gameObject.layer;
            
            // Default to Player tag if not set
            if (!CompareTag("Bee") && !CompareTag("Player")) 
            {
                colObj.tag = "Player";
            }
            else
            {
                colObj.tag = tag;
            }

            var trigger = colObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.5f;
        }
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W)) verticalInput = 1f;
        if (Input.GetKey(KeyCode.E)) verticalInput = -1f;

        // Rotation
        if (Mathf.Abs(h) > 0.01f)
        {
            Quaternion delta = Quaternion.Euler(0f, h * turnSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }

        // Forward/Backward Speed
        float targetSpeed = Mathf.Clamp(v, -1f, 1f) * maxSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed)) ? acceleration : deceleration;
        
        // Brake
        if (Input.GetKey(KeyCode.Space))
        {
            targetSpeed = 0f;
            accelRate = deceleration * 2f;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

        // Vertical Speed
        float targetVertical = Mathf.Clamp(verticalInput, -1f, 1f) * verticalSpeed;
        currentVerticalSpeed = Mathf.MoveTowards(currentVerticalSpeed, targetVertical, deceleration * Time.fixedDeltaTime);

        // Apply Velocity
        Vector3 forwardDir = invertForward ? -transform.forward : transform.forward;
        Vector3 velocity = forwardDir * currentSpeed + Vector3.up * currentVerticalSpeed;
        
        rb.linearVelocity = velocity;

        // Visual Rotation (Banking or just syncing)
        if (visualModel != null)
        {
            visualModel.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (!hasAnimator) return;

        float speedMultiplier = 1f + Mathf.Abs(currentSpeed) / maxSpeed * 1.5f;
        animator.speed = speedMultiplier;
        
        animator.SetFloat(speedHash, Mathf.Abs(currentSpeed));
        animator.SetFloat(verticalHash, currentVerticalSpeed);
    }

    public void TriggerGameOver(Transform killer)
    {
        isGameOver = true;
        rb.linearVelocity = Vector3.zero;
        enabled = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
    }
}