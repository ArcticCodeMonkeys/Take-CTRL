using UnityEngine;
using UnityEngine.InputSystem;

public class RobotScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public InputActionReference jumpAction;
    public InputActionReference moveAction;
    public InputActionReference sprintAction;

    public InputActionReference attackAction;

    public InputActionReference dodgeAction;

    public float jumpSpeed = 10;

    public float moveSpeed = 5;

    public float sprintSpeed = 10;

    public bool isGrounded = true;

    public Transform groundCheckSphere;
    public float groundCheckRadius = 0.5f; // Increased for testing
    public LayerMask groundLayer = 1 << 3; // Or set this in Inspector for better control

    private void OnEnable()
    {
        if (jumpAction?.action != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }
        if (moveAction?.action != null)
        {
            moveAction.action.Enable();
        }
        if (sprintAction?.action != null)
        {
            sprintAction.action.Enable();
        }
        if (attackAction?.action != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttack;
        }
        if (dodgeAction?.action != null)
        {
            dodgeAction.action.Enable();
            dodgeAction.action.performed += OnDodge;
        }
    }

    private void OnDisable()
    {
        if (jumpAction?.action != null)
        {
            jumpAction.action.performed -= OnJump;
            jumpAction.action.Disable();
        }
        if (moveAction?.action != null)
        {
            moveAction.action.Disable();
        }
        if (sprintAction?.action != null)
        {
            sprintAction.action.Disable();
        }
        if (attackAction?.action != null)
        {
            attackAction.action.performed -= OnAttack;
            attackAction.action.Disable();
        }
        if (dodgeAction?.action != null)
        {
            dodgeAction.action.performed -= OnDodge;
            dodgeAction.action.Disable();
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if(isGrounded) 
        {
            rb.linearVelocity = rb.linearVelocity + (Vector2.up * jumpSpeed);
        }
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        Debug.Log("Attack");
    }

    private void OnDodge(InputAction.CallbackContext ctx)
    {
        Debug.Log("Dodge");
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;
        if (moveAction?.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        bool isSprinting = false;
        if (sprintAction?.action != null)
        {
            isSprinting = sprintAction.action.IsPressed();
        }

        if (moveInput.magnitude > 0.1f)
        {
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    private void HandleJump()
    {
        
        if (checkIsGrounded())
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private bool checkIsGrounded()
    {
        if (groundCheckSphere == null)
        {
            Debug.LogWarning("Ground check sphere is null!");
            return false;
        }

       
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckSphere.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            Debug.Log($"Collider {i}: {col.gameObject.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)} ({col.gameObject.layer})");
            if (col.gameObject != this.gameObject)
            {
                Debug.Log("Grounded: true");
                return true;
            }
        }

        // Log all colliders found at the position, ignoring layer mask
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(groundCheckSphere.position, groundCheckRadius);
        for (int i = 0; i < allColliders.Length; i++)
        {
            var col = allColliders[i];
            bool isGroundLayer = (groundLayer.value & (1 << col.gameObject.layer)) != 0;
        }
        return false;
    }
    
}
