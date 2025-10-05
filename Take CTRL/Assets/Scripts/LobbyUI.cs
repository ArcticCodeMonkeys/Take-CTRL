using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;

/// <summary>
/// Handles UI interactions in the Lobby scene
/// Shows session info, player count, and start game functionality
/// Assumes networking connection is already established from Host/Join screens
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionCodeText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button copyButton;
    
    [Header("Session Management")]
    [SerializeField] private SessionCodeManager sessionCodeManager;
    
    [Header("NGO Widget References - For Display Only")]
    [SerializeField] private TMP_Text sessionInfoDisplay; // Reference to NGO Session Info Display
    
    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 4;
    
    // Track current state
    private bool isHost = false;
    private string currentSessionCode = "";
    private int currentPlayerCount = 0;
    
    private void Start()
    {
        SetupUI();
        SetupButtons();
        SetupNetworkingListeners();
    }
    
    private void SetupUI()
    {
        // Determine if we're host or client based on SceneNavigator data
        isHost = SceneNavigator.CurrentLobbyType == "Host";
        
        // Setup initial UI state
        UpdateUIForRole();
        
        // Update player count
        UpdatePlayerCount();
    }
    
    private void SetupButtons()
    {
        // Setup Start Game button (only for host)
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(isHost); // Only show for host
        }
        
        // Setup Leave button
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveClicked);
        }
        
        // Setup Copy button
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(OnCopySessionCodeClicked);
            // Initialize copy button state (will be updated when session code is generated)
            copyButton.interactable = false;
        }
    }
    
    private void SetupNetworkingListeners()
    {
        // Subscribe to connection events to track player count
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;
        }
        
        // Setup session code manager integration
        if (sessionCodeManager != null)
        {
            sessionCodeManager.OnSessionCodeUpdated += OnSessionCodeReceived;
        }
        else
        {
            // Try to find SessionCodeManager in scene
            sessionCodeManager = FindFirstObjectByType<SessionCodeManager>();
            if (sessionCodeManager != null)
            {
                sessionCodeManager.OnSessionCodeUpdated += OnSessionCodeReceived;
            }
        }
        
        // Update display with current session info
        UpdateSessionDisplay();
    }
    
    /// <summary>
    /// Called when SessionCodeManager finds a session code
    /// </summary>
    public void OnSessionCodeReceived(string sessionCode)
    {
        currentSessionCode = sessionCode;
        
        if (sessionCodeText != null && isHost)
        {
            sessionCodeText.text = $"Join Code: {sessionCode}";
        }
        
        UpdateCopyButtonState();
        Debug.Log($"LobbyUI: Received session code from SessionCodeManager: {sessionCode}");
    }
    
    private System.Collections.IEnumerator SetupNetworkingDelayed()
    {
        yield return null; // Wait one frame
        
        // Just update the display - networking should already be established
        UpdateSessionDisplay();
    }
    
    private void UpdateSessionDisplay()
    {
        // Display current session information
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            Debug.Log("LobbyUI: Confirmed we are the host with active networking");
            // For host, we can get session info from NetworkManager
            UpdateUIForConnectedHost();
        }
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            Debug.Log("LobbyUI: Confirmed we are a client with active networking");
            // For client, show joining status
            UpdateUIForConnectedClient();
        }
        else
        {
            Debug.LogWarning("LobbyUI: No active networking connection detected");
            // Not connected yet, show pending info
            UpdateUIForRole();
        }
    }
    
    private void UpdateUIForConnectedHost()
    {
        if (sessionNameText != null)
        {
            sessionNameText.text = $"Session: {SceneNavigator.PendingSessionName ?? "Host Session"}";
        }
        
        if (sessionCodeText != null)
        {
            // Generate a simple session code for the host
            string sessionCode = GenerateSessionCode();
            sessionCodeText.text = $"{sessionCode}";
            // Update copy button state since we have a new session code
            UpdateCopyButtonState();
        }
    }
    
    /// <summary>
    /// Generate a simple session code that players can use to join
    /// This is a placeholder - in a real game you'd want a proper lobby service
    /// </summary>
    private string GenerateSessionCode()
    {
        if (!string.IsNullOrEmpty(currentSessionCode))
        {
            return currentSessionCode;
        }
        
        // Generate a simple 6-character numeric code
        // Using only numbers to avoid character restrictions from networking library
        const string chars = "0123456789";
        System.Random random = new System.Random();
        currentSessionCode = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        Debug.Log($"Generated session code: {currentSessionCode}");
        return currentSessionCode;
    }
    
    private void UpdateUIForConnectedClient()
    {
        if (sessionCodeText != null)
        {
            sessionCodeText.text = $"Connected to: {SceneNavigator.PendingSessionCode ?? "Session"}";
        }
        
        if (sessionNameText != null)
        {
            sessionNameText.text = "Connected as Client";
        }
    }
    
    private void UpdateUIForRole()
    {
        if (isHost)
        {
            // Host: Show session name and generate session code immediately
            if (sessionNameText != null)
            {
                sessionNameText.text = $"Session: {SceneNavigator.PendingSessionName ?? "Unknown"}";
            }
            
            if (sessionCodeText != null)
            {
                // Generate session code immediately for host
                string sessionCode = GenerateSessionCode();
                sessionCodeText.text = $"Join Code: {sessionCode}";
                // Update copy button state since we have a new session code
                UpdateCopyButtonState();
            }
        }
        else
        {
            // Client: Show the code they're trying to join
            if (sessionCodeText != null)
            {
                sessionCodeText.text = $"Joining: {SceneNavigator.PendingSessionCode ?? "Unknown"}";
            }
            
            if (sessionNameText != null)
            {
                sessionNameText.text = "Joining session...";
            }
        }
    }
    
    private void StartHost()
    {
        Debug.Log("Host networking should already be started from HostScreenUI");
        // Verify that networking is actually active
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("Expected to be host but networking is not active!");
        }
    }
    
    private void JoinSession()
    {
        Debug.Log("Client networking should already be started from JoinScreenUI");
        // Verify that networking is actually active
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            Debug.LogError("Expected to be client but networking is not active!");
        }
    }
    
    private void UpdatePlayerCount()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            currentPlayerCount = NetworkManager.Singleton.ConnectedClients.Count;
        }
        else
        {
            currentPlayerCount = 0;
        }
        
        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {currentPlayerCount}/{maxPlayers}";
        }
        
        // Update start button state
        UpdateStartButtonState();
        
        // Update copy button state
        UpdateCopyButtonState();
    }
    
    private void UpdateStartButtonState()
    {
        if (startGameButton != null && isHost)
        {
            // Enable start button if we have at least 1 player (for testing)
            // Change to currentPlayerCount >= maxPlayers for auto-start only
            startGameButton.interactable = currentPlayerCount >= 1;
        }
    }
    
    private void UpdateCopyButtonState()
    {
        if (copyButton != null)
        {
            // Enable copy button only when there's a valid session code and we're the host
            copyButton.interactable = !string.IsNullOrEmpty(currentSessionCode) && isHost;
        }
    }
    
    #region Network Event Handlers
    
    private void OnPlayerJoined(ulong clientId)
    {
        Debug.Log($"Player joined lobby: {clientId}");
        UpdatePlayerCount();
    }
    
    private void OnPlayerLeft(ulong clientId)
    {
        Debug.Log($"Player left lobby: {clientId}");
        UpdatePlayerCount();
    }
    
    #endregion
    
    #region Button Event Handlers
    
    private void OnStartGameClicked()
    {
        if (!isHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }
        
        if (currentPlayerCount < 1)
        {
            Debug.LogWarning("Need at least 1 player to start!");
            return;
        }
        
        Debug.Log("Host starting the game...");
        
        // Restore normal player spawning before entering gameplay
        NGOCrashPrevention.RestoreNormalSpawning();
        
        // Use SceneNavigator to start the game
        SceneNavigator.Instance.StartGame();
    }
    
    private void OnLeaveClicked()
    {
        Debug.Log("Leaving lobby...");
        
        // Disconnect from networking
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Return to title screen
        SceneNavigator.NavigateToTitle();
    }
    
    private void OnCopySessionCodeClicked()
    {
        if (!string.IsNullOrEmpty(currentSessionCode))
        {
            // Copy session code to clipboard
            GUIUtility.systemCopyBuffer = currentSessionCode;
            Debug.Log($"Session code '{currentSessionCode}' copied to clipboard!");
            
            // Optional: You could add a brief UI feedback here (like showing "Copied!" text)
        }
        else
        {
            Debug.LogWarning("No session code available to copy");
        }
    }
    
    #endregion
    
    #region Public Methods for NGO Widget Integration
    
    /// <summary>
    /// Call this when connection status changes
    /// </summary>
    public void OnConnectionStatusChanged(bool connected)
    {
        if (connected)
        {
            Debug.Log("Successfully connected to session");
        }
        else
        {
            Debug.Log("Failed to connect to session");
            // Could show error message or return to previous screen
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
        if (leaveButton != null)
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
        if (copyButton != null)
            copyButton.onClick.RemoveListener(OnCopySessionCodeClicked);
        
        // Clean up session code manager events
        if (sessionCodeManager != null)
        {
            sessionCodeManager.OnSessionCodeUpdated -= OnSessionCodeReceived;
        }
        
        // Clean up network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
        }
    }
}