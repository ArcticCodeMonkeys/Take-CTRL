using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles session management, player limits, and connection approval for Take CTRL
/// Attach this to the NetworkManager GameObject in each scene
/// </summary>
public class SessionManager : NetworkBehaviour
{
    [Header("Session Settings")]
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private bool autoStartGameWhenFull = true;
    [SerializeField] private string gameSceneName = "Warehouse";
    
    [Header("Lobby Settings")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private bool transitionToLobbyOnHost = true;
    
    // Network variables to track session state
    private NetworkVariable<int> connectedPlayerCount = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);

    private void Start()
    {
        // Wait for NetworkManager to be available (since Multiplayer Widgets create it)
        StartCoroutine(WaitForNetworkManagerAndSetup());
    }
    
    private System.Collections.IEnumerator WaitForNetworkManagerAndSetup()
    {
        // Wait until NetworkManager exists (created by Multiplayer Widgets or scene load)
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }
        
        // Set up connection approval and events
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Configure DontDestroyOnLoad for Lobby/Warehouse scenes
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool shouldPersist = currentScene == lobbySceneName || currentScene == gameSceneName;
        
        if (shouldPersist)
        {
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            Debug.Log($"NetworkManager set to persist through scene changes in {currentScene}");
        }
        
        Debug.Log($"SessionManager initialized in {currentScene}");
    }

    /// <summary>
    /// Connection approval callback - enforces player limit
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Check if we have room for more players
        bool approveConnection = NetworkManager.Singleton.ConnectedClients.Count < maxPlayers && !gameStarted.Value;
        
        response.Approved = approveConnection;
        response.CreatePlayerObject = false; // We don't create individual player objects
        
        if (!approveConnection)
        {
            string reason = gameStarted.Value ? "Game already started" : "Session full";
            Debug.Log($"Connection denied: {reason}. Current players: {NetworkManager.Singleton.ConnectedClients.Count}/{maxPlayers}");
        }
        else
        {
            Debug.Log($"Connection approved. Players: {NetworkManager.Singleton.ConnectedClients.Count + 1}/{maxPlayers}");
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started - Session Manager active");
        UpdatePlayerCount();
        
        // If we're in a Host/Join screen and just started hosting, transition to lobby
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Host Screen" && transitionToLobbyOnHost)
        {
            Debug.Log("Host started from Host Screen - transitioning to Lobby");
            TransitionToLobby();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            UpdatePlayerCount();
            Debug.Log($"Player {clientId} connected. Total: {NetworkManager.Singleton.ConnectedClients.Count}/{maxPlayers}");
            
            // Check if lobby is full and should start game
            if (autoStartGameWhenFull && NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers && !gameStarted.Value)
            {
                StartGame();
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            UpdatePlayerCount();
            Debug.Log($"Player {clientId} disconnected. Total: {NetworkManager.Singleton.ConnectedClients.Count}/{maxPlayers}");
        }
    }

    private void UpdatePlayerCount()
    {
        if (IsServer)
        {
            connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    /// <summary>
    /// Manually start the game (call this from a UI button in lobby)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        StartGame();
    }

    private void StartGame()
    {
        if (!IsServer || gameStarted.Value) return;
        
        gameStarted.Value = true;
        Debug.Log("Starting game - transitioning to game scene");
        
        // Load the game scene for all connected clients
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void TransitionToLobby()
    {
        if (!IsServer) return;
        
        Debug.Log("Transitioning to lobby scene");
        NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private bool IsInLobbyOrGame()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentScene == lobbySceneName || currentScene == gameSceneName;
    }

    /// <summary>
    /// Get current player count (accessible from UI)
    /// </summary>
    public int GetPlayerCount()
    {
        return connectedPlayerCount.Value;
    }

    /// <summary>
    /// Get max players (accessible from UI)
    /// </summary>
    public int GetMaxPlayers()
    {
        return maxPlayers;
    }

    /// <summary>
    /// Check if game has started (accessible from UI)
    /// </summary>
    public bool HasGameStarted()
    {
        return gameStarted.Value;
    }

    public override void OnDestroy()
    {
        // Clean up event subscriptions
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        base.OnDestroy();
    }

    /// <summary>
    /// Leave the current session and return to title screen
    /// Call this from a Leave/Disconnect button
    /// </summary>
    public void LeaveSession()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host shutting down session");
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Client disconnecting from session");
                NetworkManager.Singleton.Shutdown();
            }
        }
        
        // Wait a moment then return to title screen
        StartCoroutine(ReturnToTitleAfterDisconnect());
    }
    
    private System.Collections.IEnumerator ReturnToTitleAfterDisconnect()
    {
        // Wait for network shutdown to complete
        yield return new WaitForSeconds(1f);
        
        // Load title screen (non-networked)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }
    
    // UI Helper methods for displaying session info
    public string GetSessionStatus()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
            return "Not Connected";
        
        if (gameStarted.Value)
            return "Game In Progress";
        
        return $"Lobby ({connectedPlayerCount.Value}/{maxPlayers})";
    }
}