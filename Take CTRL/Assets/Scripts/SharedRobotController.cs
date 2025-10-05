using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Simplified networked robot controller where all connected players contribute equally to movement
/// </summary>
public class SharedRobotController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpSpeed = 15f;
    
    [Header("Input References")]
    public InputActionReference moveAction;
    public InputActionReference sprintAction;
    public InputActionReference jumpAction;
    public InputActionReference attackAction;
    public InputActionReference dodgeAction;
    
    [Header("Ground Check")]
    public Transform groundCheckSphere;
    public float groundCheckRadius = 0.5f;
    public LayerMask groundLayer = 1 << 3;
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    
    // Network variables for combined movement
    private NetworkVariable<Vector2> combinedMovement = new NetworkVariable<Vector2>();
    private NetworkVariable<bool> combinedSprint = new NetworkVariable<bool>();
    private NetworkVariable<bool> isGroundedNet = new NetworkVariable<bool>();
    
    // Client input tracking on server (simplified)
    private Dictionary<ulong, Vector2> clientInputs = new Dictionary<ulong, Vector2>();
    private Dictionary<ulong, bool> clientSprints = new Dictionary<ulong, bool>();
    
    // Local input tracking
    private Vector2 localMovementInput;
    private bool localSprintInput;
    private bool isGrounded;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    public override void OnNetworkSpawn()
    {
        // Enable input for all clients
        EnableInputActions();
        
        // Subscribe to network variable changes for movement
        combinedMovement.OnValueChanged += OnCombinedMovementChanged;
        combinedSprint.OnValueChanged += OnCombinedSprintChanged;
        isGroundedNet.OnValueChanged += OnGroundedChanged;
        
        Debug.Log($"SharedRobotController spawned. IsServer: {IsServer}, IsClient: {IsClient}");
    }
    
    public override void OnNetworkDespawn()
    {
        DisableInputActions();
        
        // Unsubscribe from network variable changes
        combinedMovement.OnValueChanged -= OnCombinedMovementChanged;
        combinedSprint.OnValueChanged -= OnCombinedSprintChanged;
        isGroundedNet.OnValueChanged -= OnGroundedChanged;
    }
    
    private void EnableInputActions()
    {
        if (moveAction?.action != null)
            moveAction.action.Enable();
        if (sprintAction?.action != null)
            sprintAction.action.Enable();
        if (jumpAction?.action != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpInput;
        }
        if (attackAction?.action != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackInput;
        }
        if (dodgeAction?.action != null)
        {
            dodgeAction.action.Enable();
            dodgeAction.action.performed += OnDodgeInput;
        }
    }
    
    private void DisableInputActions()
    {
        if (moveAction?.action != null)
            moveAction.action.Disable();
        if (sprintAction?.action != null)
            sprintAction.action.Disable();
        if (jumpAction?.action != null)
        {
            jumpAction.action.performed -= OnJumpInput;
            jumpAction.action.Disable();
        }
        if (attackAction?.action != null)
        {
            attackAction.action.performed -= OnAttackInput;
            attackAction.action.Disable();
        }
        if (dodgeAction?.action != null)
        {
            dodgeAction.action.performed -= OnDodgeInput;
            dodgeAction.action.Disable();
        }
    }
    
    private void Update()
    {
        // Read local input
        ReadLocalInput();
        
        // Send input to server
        if (IsClient)
        {
            SendInputToServerRpc(localMovementInput, localSprintInput);
        }
        
        // Update ground check on server
        if (IsServer)
        {
            UpdateGroundCheck();
        }
    }
    
    private void FixedUpdate()
    {
        // Apply movement based on combined input
        if (IsServer)
        {
            ApplyMovement();
        }
    }
    
    private void ReadLocalInput()
    {
        // Read movement input
        if (moveAction?.action != null)
            localMovementInput = moveAction.action.ReadValue<Vector2>();
        else
            localMovementInput = Vector2.zero;
            
        // Read sprint input
        if (sprintAction?.action != null)
            localSprintInput = sprintAction.action.IsPressed();
        else
            localSprintInput = false;
    }
    
    [Rpc(SendTo.Server)]
    private void SendInputToServerRpc(Vector2 movement, bool sprint)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        
        // Update this client's input
        clientInputs[clientId] = movement;
        clientSprints[clientId] = sprint;
        
        // Calculate combined input from all clients
        CalculateCombinedInput();
    }
    
    private void CalculateCombinedInput()
    {
        if (!IsServer) return;
        
        int clientCount = NetworkManager.Singleton.ConnectedClients.Count;
        if (clientCount == 0) return;
        
        Vector2 totalMovement = Vector2.zero;
        int sprintCount = 0;
        
        // Sum all client inputs
        foreach (var kvp in clientInputs)
        {
            totalMovement += kvp.Value;
            if (clientSprints.ContainsKey(kvp.Key) && clientSprints[kvp.Key])
                sprintCount++;
        }
        
        // Average the movement (each client has equal weight)
        Vector2 averageMovement = totalMovement / clientCount;
        
        // Sprint if majority wants to sprint
        bool shouldSprint = sprintCount > (clientCount / 2);
        
        // Update network variables
        combinedMovement.Value = averageMovement;
        combinedSprint.Value = shouldSprint;
    }
    
    private void ApplyMovement()
    {
        if (rb == null) return;
        
        Vector2 movement = combinedMovement.Value;
        bool sprint = combinedSprint.Value;
        
        // Handle horizontal movement
        if (movement.magnitude > 0.1f)
        {
            float currentSpeed = sprint ? sprintSpeed : moveSpeed;
            Vector2 targetVelocity = new Vector2(movement.x * currentSpeed, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
            
            // Update animator
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
                
                // Face direction
                if (movement.x > 0)
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (movement.x < 0)
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            
            if (animator != null)
                animator.SetBool("isMoving", false);
        }
    }
    
    private void UpdateGroundCheck()
    {
        bool grounded = CheckIsGrounded();
        isGroundedNet.Value = grounded;
        isGrounded = grounded;
    }
    
    private bool CheckIsGrounded()
    {
        if (groundCheckSphere == null) return false;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckSphere.position, groundCheckRadius, groundLayer);
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject != this.gameObject)
                return true;
        }
        
        return false;
    }
    
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        if (IsClient && isGroundedNet.Value)
        {
            TriggerJumpRpc();
        }
    }
    
    private void OnAttackInput(InputAction.CallbackContext context)
    {
        if (IsClient)
        {
            TriggerAttackRpc();
        }
    }
    
    private void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (IsClient)
        {
            TriggerDodgeRpc();
        }
    }
    
    [Rpc(SendTo.Server)]
    private void TriggerJumpRpc()
    {
        if (isGrounded && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
            Debug.Log("Robot jumped!");
        }
    }
    
    [Rpc(SendTo.Server)]
    private void TriggerAttackRpc()
    {
        Debug.Log("Robot attacked!");
        // Implement attack logic here
    }
    
    [Rpc(SendTo.Server)]
    private void TriggerDodgeRpc()
    {
        Debug.Log("Robot dodged!");
        // Implement dodge logic here
    }
    
    // Network variable change callbacks
    private void OnCombinedMovementChanged(Vector2 oldValue, Vector2 newValue)
    {
        // Movement is handled in FixedUpdate on server
    }
    
    private void OnCombinedSprintChanged(bool oldValue, bool newValue)
    {
        // Sprint state changed
    }
    
    private void OnGroundedChanged(bool oldValue, bool newValue)
    {
        isGrounded = newValue;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheckSphere != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckSphere.position, groundCheckRadius);
        }
    }
}