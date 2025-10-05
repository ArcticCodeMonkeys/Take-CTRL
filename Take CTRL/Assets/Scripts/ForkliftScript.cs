using Unity.Netcode;
using UnityEngine;

public class ForkliftScript : NetworkBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
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
        // Always Left
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    }
}
