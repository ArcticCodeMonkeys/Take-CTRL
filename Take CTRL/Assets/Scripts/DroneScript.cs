using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform playerTransform;
    public float moveSpeed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Move towards the player smoothly
        if (playerTransform == null) return;
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 targetVelocity = direction * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 2f);
        
    }
}
