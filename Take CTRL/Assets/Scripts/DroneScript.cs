using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform playerTransform;
    public float moveSpeed = 2f;
    public float despawnDelay = 5f; // Time in seconds before despawning when invisible
    
    private bool isVisible = false;
    private bool hasBeenActivated = false;
    private float invisibleTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Only move if the drone has been visible at least once
        if (hasBeenActivated)
        {
            HandleMovement();
            
            // Track time while invisible and despawn if needed
            if (!isVisible)
            {
                invisibleTimer += Time.deltaTime;
                if (invisibleTimer >= despawnDelay)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void HandleMovement()
    {
        // Move towards the player smoothly
        if (playerTransform == null) return;
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 targetVelocity = direction * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 2f);
    }
    
    void OnBecameVisible()
    {
        isVisible = true;
        hasBeenActivated = true;
        invisibleTimer = 0f; // Reset timer when becoming visible
    }
    
    void OnBecameInvisible()
    {
        isVisible = false;
        invisibleTimer = 0f; // Start counting from when it becomes invisible
    }
}
