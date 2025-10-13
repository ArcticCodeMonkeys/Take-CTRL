using UnityEngine;

/// <summary>
/// Controls individual drone behavior within a swarm.
/// Each drone follows the swarm center transform while adding random variance to its movement.
/// </summary>
public class SwarmDroneScript : MonoBehaviour
{
    [Header("Swarm Settings")]
    [SerializeField] private Transform swarmCenter;
    [SerializeField] private Vector3 relativePosition;
    [SerializeField] private bool autoSetRelativePosition = true;
    
    [Header("Movement Variance")]
    [SerializeField] private float horizontalVariance = 2f;
    [SerializeField] private float verticalVariance = 1f;
    [SerializeField] private float varianceSpeed = 1f;
    [SerializeField] private float smoothingSpeed = 2f;
    
    [Header("Animation")]
    [SerializeField] private float bobAmount = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    
    // Private variables
    private Vector3 targetPosition;
    private Vector3 randomOffset;
    private float bobOffset;
    private float timeOffset;
    
    void Start()
    {
        // If no swarm center is assigned, try to find the parent Swarm object
        if (swarmCenter == null)
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.name.Contains("Swarm"))
                {
                    swarmCenter = parent;
                    break;
                }
                parent = parent.parent;
            }
        }
        
        // Auto-set relative position based on current position
        if (autoSetRelativePosition && swarmCenter != null)
        {
            relativePosition = transform.position - swarmCenter.position;
        }
        
        // Initialize random values
        timeOffset = Random.Range(0f, 10f);
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        
        Debug.Log($"Drone {gameObject.name} initialized with swarm center: {(swarmCenter ? swarmCenter.name : "None")}");
    }
    
    void Update()
    {
        if (swarmCenter == null)
        {
            Debug.LogWarning($"SwarmDroneScript on {gameObject.name} has no swarm center assigned!");
            return;
        }
        
        UpdateRandomVariance();
        UpdateTargetPosition();
        MoveTowardsTarget();
    }
    
    private void UpdateRandomVariance()
    {
        float time = Time.time + timeOffset;
        
        // Generate smooth random movement using Perlin noise (2D focused)
        float randomX = (Mathf.PerlinNoise(time * varianceSpeed, 0f) - 0.5f) * horizontalVariance * 2f;
        float randomY = (Mathf.PerlinNoise(0f, time * varianceSpeed) - 0.5f) * verticalVariance * 2f;
        
        // Add bobbing motion
        float bob = Mathf.Sin((time + bobOffset) * bobSpeed) * bobAmount;
        randomY += bob;
        
        // For 2D, we only use X and Y movement (Z stays at 0)
        randomOffset = new Vector3(randomX, randomY, 0f);
    }
    
    private void UpdateTargetPosition()
    {
        // Calculate base position relative to swarm center
        Vector3 basePosition = swarmCenter.position + relativePosition;
        
        // Add random variance
        targetPosition = basePosition + randomOffset;
    }
    
    private void MoveTowardsTarget()
    {
        // Smoothly move towards target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingSpeed * Time.deltaTime);
        
        // For 2D games, we don't need rotation - drones maintain their original orientation
    }
    
    /// <summary>
    /// Set the swarm center transform manually
    /// </summary>
    public void SetSwarmCenter(Transform center)
    {
        swarmCenter = center;
        if (autoSetRelativePosition)
        {
            relativePosition = transform.position - swarmCenter.position;
        }
    }
    
    /// <summary>
    /// Set the relative position manually
    /// </summary>
    public void SetRelativePosition(Vector3 position)
    {
        relativePosition = position;
        autoSetRelativePosition = false;
    }
    
    /// <summary>
    /// Get the current relative position
    /// </summary>
    public Vector3 GetRelativePosition()
    {
        return relativePosition;
    }
    
    void OnDrawGizmosSelected()
    {
        if (swarmCenter != null)
        {
            // Draw line to swarm center
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, swarmCenter.position);
            
            // Draw relative position
            Gizmos.color = Color.green;
            Vector3 relativePos = swarmCenter.position + relativePosition;
            Gizmos.DrawWireSphere(relativePos, 0.2f);
            
            // Draw variance area (2D rectangle)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(relativePos, new Vector3(horizontalVariance * 2f, verticalVariance * 2f, 0.1f));
        }
    }
}