using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBehaviour : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public bool isGameOver;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 20f;
    [SerializeField] private float height = 8f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float lookAheadDistance = 5f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private LayerMask collisionLayers = -1;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (isGameOver || target == null) return;

        HandleCameraMovement();
    }

    private void HandleCameraMovement()
    {
        // Calculate desired position
        // Bee moves forward, but model might be inverted. Assuming +forward is 'behind' for the camera relative to movement.
        Vector3 targetOffset = target.forward * distance + Vector3.up * height;
        Vector3 desiredPosition = target.position + targetOffset;

        // Check for obstacles
        Vector3 directionToCamera = desiredPosition - target.position;
        float checkDistance = directionToCamera.magnitude;

        if (Physics.Raycast(target.position, directionToCamera.normalized, out RaycastHit hit, checkDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // If we hit something that isn't the target or a player/enemy
            if (!IsIgnoredObject(hit.collider))
            {
                // Move camera closer to avoid clipping
                float blockedDistance = Mathf.Max(hit.distance - 0.5f, minDistance);
                desiredPosition = target.position + directionToCamera.normalized * blockedDistance;
            }
        }

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Look at target with offset
        Vector3 lookAtTarget = target.position - target.forward * lookAheadDistance;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private bool IsIgnoredObject(Collider col)
    {
        // Ignore the player/target itself and specific entities
        if (col.transform == target) return true;
        if (col.CompareTag("Player") || col.CompareTag("Wasp")) return true;
        
        return false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}