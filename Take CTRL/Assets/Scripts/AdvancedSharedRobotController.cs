using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Advanced shared character controller that properly combines input from all 4 players
/// Each player contributes exactly 0.25 (25%) weight to the final movement
/// </summary>
public class AdvancedSharedRobotController : NetworkBehaviour
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
    
    // Player input tracking (Server only)
    private Dictionary<ulong, PlayerInput> playerInputs = new Dictionary<ulong, PlayerInput>();
    
    // Networked state
    private NetworkVariable<Vector2> finalMovement = new NetworkVariable<Vector2>();
    private NetworkVariable<bool> finalSprint = new NetworkVariable<bool>();
    private NetworkVariable<bool> isGrounded = new NetworkVariable<bool>();
    
    // Constants
    private const int MAX_PLAYERS = 4;
    private const float PLAYER_WEIGHT = 0.25f; // Each player contributes 25%
    
    /// <summary>
    /// Structure to hold individual player input
    /// </summary>
    [System.Serializable]
    public struct PlayerInput
    {
        public Vector2 movement;
        public bool sprint;
        public float lastUpdateTime;
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public override void OnNetworkSpawn()
    {
        // All clients enable input, but only server processes combination
        EnableInputActions();
        
        // Subscribe to network variable changes for movement application
        finalMovement.OnValueChanged += OnFinalMovementChanged;
        finalSprint.OnValueChanged += OnFinalSprintChanged;
        
        Debug.Log($"AdvancedSharedRobotController spawned for ClientId: {OwnerClientId}");
    }
    
    public override void OnNetworkDespawn()
    {
        DisableInputActions();
        
        // Clean up
        finalMovement.OnValueChanged -= OnFinalMovementChanged;
        finalSprint.OnValueChanged -= OnFinalSprintChanged;
        
        // Remove this player's input from tracking (Server only)
        if (IsServer && playerInputs.ContainsKey(OwnerClientId))
        {
            playerInputs.Remove(OwnerClientId);
            RecalculateMovement();
        }
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
        // All clients send their input to server
        SendInputToServer();
        
        // Server updates ground check
        if (IsServer)
        {
            UpdateGroundCheck();
        }
    }
    
    private void FixedUpdate()
    {
        // Only server applies movement
        if (IsServer)
        {
            ApplyMovement();
        }
    }
    
    private void SendInputToServer()
    {
        // Read current input
        Vector2 movementInput = Vector2.zero;
        bool sprintInput = false;
        
        if (moveAction?.action != null)
        {
            movementInput = moveAction.action.ReadValue<Vector2>();
        }
        
        if (sprintAction?.action != null)
        {
            sprintInput = sprintAction.action.IsPressed();
        }
        
        // Send to server
        UpdatePlayerInputRpc(movementInput, sprintInput);
    }
    
    /// <summary>
    /// RPC to update this player's input on the server
    /// </summary>
    [Rpc(SendTo.Server)]
    private void UpdatePlayerInputRpc(Vector2 movementInput, bool sprintInput)
    {
        // Use the owner client ID since this RPC is sent from the owner
        ulong senderId = OwnerClientId;
        
        // Update this player's input
        PlayerInput input = new PlayerInput
        {
            movement = movementInput,
            sprint = sprintInput,
            lastUpdateTime = Time.time
        };
        
        playerInputs[senderId] = input;
        
        // Recalculate combined movement
        RecalculateMovement();
    }
    
    /// <summary>
    /// Combine all players' inputs with equal weight (0.25 each)
    /// </summary>
    private void RecalculateMovement()
    {
        if (!IsServer) return;
        
        Vector2 combinedMovement = Vector2.zero;
        int sprintVotes = 0;
        int activePlayerCount = 0;
        
        // Combine input from all active players
        foreach (var kvp in playerInputs)
        {
            PlayerInput input = kvp.Value;
            
            // Only count recent inputs (within last 0.1 seconds)
            if (Time.time - input.lastUpdateTime < 0.1f)
            {
                combinedMovement += input.movement * PLAYER_WEIGHT;
                if (input.sprint) sprintVotes++;
                activePlayerCount++;
            }
        }
        
        // Determine if character should sprint
        // Sprint if majority of active players want to sprint
        bool shouldSprint = sprintVotes > (activePlayerCount / 2f);
        
        // Clamp combined movement to prevent super-fast movement with multiple inputs
        combinedMovement = Vector2.ClampMagnitude(combinedMovement, 1f);
        
        // Update network variables
        finalMovement.Value = combinedMovement;
        finalSprint.Value = shouldSprint;
        
        // Debug info
        Debug.Log($"Combined Movement: {combinedMovement}, Sprint: {shouldSprint}, Active Players: {activePlayerCount}");
    }
    
    /// <summary>
    /// Apply the final calculated movement to the character
    /// </summary>
    private void ApplyMovement()
    {
        Vector2 movement = finalMovement.Value;
        bool shouldSprint = finalSprint.Value;
        
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
        JumpInputRpc();
    }
    
    private void OnAttackInput(InputAction.CallbackContext ctx)
    {
        AttackInputRpc();
    }
    
    private void OnDodgeInput(InputAction.CallbackContext ctx)
    {
        DodgeInputRpc();
    }
    
    /// <summary>
    /// Jump input - any player can trigger jump if grounded
    /// </summary>
    [Rpc(SendTo.Server)]
    private void JumpInputRpc()
    {
        if (isGrounded.Value)
        {
            rb.linearVelocity = rb.linearVelocity + (Vector2.up * jumpSpeed);
            Debug.Log($"Character jumped!");
        }
    }
    
    /// <summary>
    /// Attack input - any player can trigger attack
    /// </summary>
    [Rpc(SendTo.Server)]
    private void AttackInputRpc()
    {
        Debug.Log($"Attack triggered!");
        
        // Broadcast attack to all clients for visual effects
        ShowAttackEffectRpc();
    }
    
    /// <summary>
    /// Dodge input - any player can trigger dodge
    /// </summary>
    [Rpc(SendTo.Server)]
    private void DodgeInputRpc()
    {
        Debug.Log($"Dodge triggered!");
        
        // Broadcast dodge to all clients for visual effects
        ShowDodgeEffectRpc();
    }
    
    /// <summary>
    /// Show attack effect on all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowAttackEffectRpc()
    {
        Debug.Log("Attack effect played!");
        // Add visual/audio effects for attack here
    }
    
    /// <summary>
    /// Show dodge effect on all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowDodgeEffectRpc()
    {
        Debug.Log("Dodge effect played!");
        // Add visual/audio effects for dodge here
    }
    
    #endregion
    
    #region Network Variable Change Handlers
    
    private void OnFinalMovementChanged(Vector2 previousValue, Vector2 newValue)
    {
        // Update animations or visual feedback based on movement
    }
    
    private void OnFinalSprintChanged(bool previousValue, bool newValue)
    {
        // Update animations or visual feedback based on sprint state
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get current number of active players contributing to movement
    /// </summary>
    public int GetActivePlayerCount()
    {
        if (!IsServer) return 0;
        
        int count = 0;
        foreach (var kvp in playerInputs)
        {
            if (Time.time - kvp.Value.lastUpdateTime < 0.1f)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Get the current combined movement vector
    /// </summary>
    public Vector2 GetCombinedMovement()
    {
        return finalMovement.Value;
    }
    
    #endregion
}