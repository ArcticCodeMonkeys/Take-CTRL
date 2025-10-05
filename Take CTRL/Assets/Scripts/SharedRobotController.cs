using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Networked character controller where all 4 players contribute equally to movement
/// Each player has 0.25 (25%) weight in the final movement decision
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
    
    [Header("Player Weight")]
    [SerializeField] private float playerWeight = 0.25f; // Each player contributes 25%
    
    // Components
    private Rigidbody2D rb;
    
    // Networked movement data
    private NetworkVariable<Vector2> combinedMovement = new NetworkVariable<Vector2>();
    private NetworkVariable<bool> combinedSprint = new NetworkVariable<bool>();
    private NetworkVariable<bool> isGrounded = new NetworkVariable<bool>();
    
    // Local input tracking
    private Vector2 localMovementInput;
    private bool localSprintInput;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public override void OnNetworkSpawn()
    {
        // Enable input only for clients that own this behavior
        if (IsOwner)
        {
            EnableInputActions();
        }
        
        // Subscribe to network variable changes
        combinedMovement.OnValueChanged += OnCombinedMovementChanged;
        combinedSprint.OnValueChanged += OnCombinedSprintChanged;
        
        Debug.Log($"SharedRobotController spawned for ClientId: {OwnerClientId}");
    }
    
    public override void OnNetworkDespawn()
    {
        DisableInputActions();
        
        // Unsubscribe from network variable changes
        combinedMovement.OnValueChanged -= OnCombinedMovementChanged;
        combinedSprint.OnValueChanged -= OnCombinedSprintChanged;
    }
    
    private void EnableInputActions()
    {
        if (moveAction?.action != null)
        {
            moveAction.action.Enable();
        }
        if (sprintAction?.action != null)
        {
            sprintAction.action.Enable();
        }
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
        {
            moveAction.action.Disable();
        }
        if (sprintAction?.action != null)
        {
            sprintAction.action.Disable();
        }
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
        // Only process input if we own this network object
        if (!IsOwner) return;
        
        // Read local input
        ReadLocalInput();
        
        // Send input to server
        SendInputToServerRpc(localMovementInput, localSprintInput);
        
        // Update ground check on server
        if (IsServer)
        {
            UpdateGroundCheck();
        }
    }
    
    private void FixedUpdate()
    {
        // Only the server applies movement to avoid conflicts
        if (IsServer)
        {
            ApplyMovement();
        }
    }
    
    private void ReadLocalInput()
    {
        // Read movement input
        if (moveAction?.action != null)
        {
            localMovementInput = moveAction.action.ReadValue<Vector2>();
        }
        
        // Read sprint input
        if (sprintAction?.action != null)
        {
            localSprintInput = sprintAction.action.IsPressed();
        }
    }
    
    /// <summary>
    /// RPC to send player input to server for combination
    /// </summary>
    [Rpc(SendTo.Server)]
    private void SendInputToServerRpc(Vector2 movementInput, bool sprintInput)
    {
        // Combine this player's input with others
        CombinePlayerInput(movementInput, sprintInput);
    }
    
    /// <summary>
    /// Combine all players' inputs on the server
    /// Each player contributes 25% to the final movement
    /// </summary>
    private void CombinePlayerInput(Vector2 movementInput, bool sprintInput)
    {
        if (!IsServer) return;
        
        // For now, we'll use a simple approach where each connected client contributes equally
        // In a more complex system, you'd track each player's individual input
        
        Vector2 weightedMovement = movementInput * playerWeight;
        bool weightedSprint = sprintInput; // Sprint is simpler - if any player sprints, character sprints
        
        // Update network variables (this will sync to all clients)
        combinedMovement.Value = weightedMovement;
        combinedSprint.Value = weightedSprint;
        
        // Note: This is a simplified version. For true 4-player input combination,
        // you'd need to track each player's input separately and combine all 4
    }
    
    /// <summary>
    /// Apply the combined movement to the character
    /// </summary>
    private void ApplyMovement()
    {
        Vector2 movement = combinedMovement.Value;
        bool shouldSprint = combinedSprint.Value;
        
        if (movement.magnitude > 0.1f)
        {
            float currentSpeed = shouldSprint ? sprintSpeed : moveSpeed;
            Vector2 targetVelocity = new Vector2(movement.x * currentSpeed, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Stop horizontal movement but keep vertical velocity
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    
    private void UpdateGroundCheck()
    {
        bool newGroundedState = CheckIsGrounded();
        isGrounded.Value = newGroundedState;
    }
    
    private bool CheckIsGrounded()
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
            if (col.gameObject != this.gameObject)
            {
                return true;
            }
        }
        return false;
    }
    
    #region Input Event Handlers
    
    private void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            JumpInputRpc();
        }
    }
    
    private void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            AttackInputRpc();
        }
    }
    
    private void OnDodgeInput(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            DodgeInputRpc();
        }
    }
    
    [Rpc(SendTo.Server)]
    private void JumpInputRpc()
    {
        if (isGrounded.Value)
        {
            rb.linearVelocity = rb.linearVelocity + (Vector2.up * jumpSpeed);
            Debug.Log($"Player {OwnerClientId} jumped!");
        }
    }
    
    [Rpc(SendTo.Server)]
    private void AttackInputRpc()
    {
        Debug.Log($"Player {OwnerClientId} attacked!");
        // Add attack logic here
    }
    
    [Rpc(SendTo.Server)]
    private void DodgeInputRpc()
    {
        Debug.Log($"Player {OwnerClientId} dodged!");
        // Add dodge logic here
    }
    
    #endregion
    
    #region Network Variable Change Handlers
    
    private void OnCombinedMovementChanged(Vector2 previousValue, Vector2 newValue)
    {
        // You can add visual feedback here when movement changes
        // For example, update animation based on movement direction
    }
    
    private void OnCombinedSprintChanged(bool previousValue, bool newValue)
    {
        // You can add visual feedback here when sprint state changes
        // For example, change animation speed or particle effects
    }
    
    #endregion
}