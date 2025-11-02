using UnityEngine;

public class ForkliftScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
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
        // Only move if the forklift has been visible at least once
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
        else
        {
            // Keep velocity at zero until activated
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        // Always Left
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
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
