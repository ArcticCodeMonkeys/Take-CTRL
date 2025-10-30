using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections;
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
    
    // RPC throttling
    private Vector2 lastSentInput;
    private bool lastSentSprint;
    private float lastInputSendTime;
    private bool isGrounded;
    private string logFilePath;
    
    private void LogToFile(string message)
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] Client {NetworkManager.Singleton.LocalClientId}: {message}\n";
            System.IO.File.AppendAllText(logFilePath, logEntry);
        }
        catch { /* Ignore file errors */ }
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Subscribe to scene loaded events to manage camera state
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene loaded events
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when a scene is loaded - manages camera state based on scene type
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        bool isInGameScene = (scene.name == "Warehouse" || scene.name == "Level1");
        
        // Disable robot camera if not in a game scene (e.g., in Lobby or Lose)
        Camera robotCamera = GetComponentInChildren<Camera>();
        if (robotCamera != null)
        {
            robotCamera.enabled = isInGameScene;
            Debug.Log($"📷 Scene changed to {scene.name} - Robot camera {(isInGameScene ? "enabled" : "disabled")}");
        }
    }
    
    public override void OnNetworkSpawn()
    {
        Debug.Log($"🎮 SharedRobotController.OnNetworkSpawn() - IsServer: {IsServer}, IsClient: {IsClient}");
        
        // Check current scene to determine if we should enable/disable camera
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isInGameScene = (currentScene == "Warehouse" || currentScene == "Level1");
        
        // Manage camera state based on scene
        Camera robotCamera = GetComponentInChildren<Camera>();
        if (robotCamera != null)
        {
            robotCamera.enabled = isInGameScene;
            Debug.Log($"📷 Robot camera {(isInGameScene ? "enabled" : "disabled")} for scene: {currentScene}");
        }
        
        // Ensure player visuals are shown (in case they were hidden from a previous Lose scene)
        // But only if we're in a game scene
        if (isInGameScene)
        {
            ShowPlayerVisuals();
        }
        
        // Initialize log file
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        logFilePath = System.IO.Path.Combine(documentsPath, $"TakeCTRL_Client{NetworkManager.Singleton.LocalClientId}_Logs.txt");
        LogToFile($"🎮 SharedRobotController spawned - IsServer: {IsServer}, IsClient: {IsClient}");
        
        // Enable input for all clients
        EnableInputActions();
        
        // Subscribe to network variable changes for movement
        combinedMovement.OnValueChanged += OnCombinedMovementChanged;
        combinedSprint.OnValueChanged += OnCombinedSprintChanged;
        isGroundedNet.OnValueChanged += OnGroundedChanged;
        
        Debug.Log($"✅ SharedRobotController spawned. IsServer: {IsServer}, IsClient: {IsClient}");
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
        // ALL clients should read input and send to server (not just owner)
        ReadLocalInput();
        
        // Send input to server with throttling (only when input changes or every 0.1 seconds)
        if (IsClient)
        {
            bool inputChanged = (localMovementInput != lastSentInput) || (localSprintInput != lastSentSprint);
            bool timeToSend = Time.time - lastInputSendTime > 0.1f;
            
            if (inputChanged || timeToSend)
            {
                if (localMovementInput.magnitude > 0.1f || inputChanged)
                {
                    string message = $"🎮 SENDING RPC: movement={localMovementInput}, sprint={localSprintInput}";
                    Debug.Log($"🎮 CLIENT {NetworkManager.Singleton.LocalClientId} {message}");
                    LogToFile(message);
                    SendInputToServerRpc(localMovementInput, localSprintInput);
                    
                    lastSentInput = localMovementInput;
                    lastSentSprint = localSprintInput;
                    lastInputSendTime = Time.time;
                }
            }
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
        // Both server and clients need to apply movement for smooth synchronization
        ApplyMovement();
    }
    
    private void ReadLocalInput()
    {
        Vector2 previousInput = localMovementInput;
        
        // Read movement input
        if (moveAction?.action != null)
        {
            localMovementInput = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            localMovementInput = Vector2.zero;
            if (Time.frameCount % 120 == 0) // Log once every 2 seconds
            {
                Debug.LogWarning("🚨 moveAction is null!");
            }
        }
            
        // Read sprint input
        if (sprintAction?.action != null)
            localSprintInput = sprintAction.action.IsPressed();
        else
            localSprintInput = false;
            
        // Debug log when input changes (so we can see if client is detecting input)
        if (localMovementInput != previousInput && localMovementInput.magnitude > 0.1f)
        {
            string message = $"🎮 detected input change: {localMovementInput}";
            Debug.Log($"🎮 Client {NetworkManager.Singleton.LocalClientId} {message}");
            LogToFile(message);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void SendInputToServerRpc(Vector2 movement, bool sprint)
    {
        // Simple approach: use a counter for client identification
        // In a real game, you'd use proper client ID tracking
        ulong senderId = 0; // We'll fix this properly later
        
        // For now, let's just find an available slot or use client count
        if (!clientInputs.ContainsKey(senderId))
        {
            senderId = (ulong)clientInputs.Count;
        }
        
        Debug.Log($"🎮 SERVER RECEIVED INPUT from client {senderId}: movement={movement}, sprint={sprint}");
        
        // Update this client's input
        clientInputs[senderId] = movement;
        clientSprints[senderId] = sprint;
        
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
        
        // Debug log movement application
        if (movement.magnitude > 0.1f && Time.frameCount % 30 == 0) // Log every 0.5 seconds when moving
        {
            string message = $"🏃 APPLYING movement: {movement}, sprint: {sprint}, isServer: {IsServer}";
            Debug.Log($"🏃 {(IsServer ? "SERVER" : "CLIENT")} {NetworkManager.Singleton.LocalClientId} {message}");
            LogToFile(message);
        }
        
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
        if (IsClient)
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
        // When movement changes, apply it immediately for responsive client updates
        if (!IsServer)
        {
            string message = $"🔄 CLIENT RECEIVED movement update: {oldValue} → {newValue}";
            Debug.Log($"🔄 Client {NetworkManager.Singleton.LocalClientId} {message}");
            LogToFile(message);
        }
    }
    
    private void OnCombinedSprintChanged(bool oldValue, bool newValue)
    {
        // Sprint state changed
    }
    
    private void OnGroundedChanged(bool oldValue, bool newValue)
    {
        isGrounded = newValue;
    }
    
    /// <summary>
    /// Kill the robot (called when hit by dangerous objects like swarm drones)
    /// </summary>
    public void Die()
    {
        if (IsServer)
        {
            TriggerDeathRpc();
        }
    }
    
    /// <summary>
    /// RPC to handle death on all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerDeathRpc()
    {
        Debug.Log("🔥 Robot has died! Switching to Lose scene...");
        
        // Disable movement by setting zero velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Disable input actions
        DisableInputActions();
        
        // Switch to Lose scene after a short delay
        StartCoroutine(SwitchToLoseSceneAfterDelay(1f));
    }
    
    /// <summary>
    /// Coroutine to switch to the Lose scene after death
    /// </summary>
    private System.Collections.IEnumerator SwitchToLoseSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Hide the player visuals before switching scenes
        HidePlayerVisuals();
        
        // Only the server should handle scene switching in networked games
        if (IsServer && NetworkManager.Singleton != null)
        {
            Debug.Log("🔄 Switching all clients to Lose scene...");
            
            // Use NetworkManager to switch scene for all clients
            NetworkManager.Singleton.SceneManager.LoadScene("Lose", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    
    /// <summary>
    /// Hides the player's visual components (needed for camera but invisible in Lose scene)
    /// </summary>
    private void HidePlayerVisuals()
    {
        // Disable all SpriteRenderers on this object and its children
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = false;
        }
        
        // Also disable MeshRenderers if any
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }
        
        Debug.Log("👻 Player visuals hidden for Lose scene");
    }
    
    /// <summary>
    /// Shows the player's visual components (called when starting a new game)
    /// </summary>
    private void ShowPlayerVisuals()
    {
        // Re-enable all SpriteRenderers on this object and its children
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = true;
        }
        
        // Also re-enable MeshRenderers if any
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = true;
        }
        
        Debug.Log("✨ Player visuals shown for new game");
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