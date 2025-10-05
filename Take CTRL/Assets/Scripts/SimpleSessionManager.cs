using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple session manager that works with NGO Multiplayer Widgets
/// Only handles 4-player limit and scene transitions
/// </summary>
public class SimpleSessionManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Warehouse";
    [SerializeField] private int maxPlayers = 4;
    
    public static SimpleSessionManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Subscribe to NetworkManager events when it becomes available
        if (NetworkManager.Singleton != null)
        {
            SetupNetworkCallbacks();
        }
        else
        {
            // Wait for NetworkManager to be ready
            StartCoroutine(WaitForNetworkManager());
        }
    }
    
    private System.Collections.IEnumerator WaitForNetworkManager()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }
        SetupNetworkCallbacks();
    }
    
    private void SetupNetworkCallbacks()
    {
        var networkManager = NetworkManager.Singleton;
        
        // Configure connection approval to enforce max players
        networkManager.ConnectionApprovalCallback = ApprovalCheck;
        
        // Subscribe to player connection events
        networkManager.OnClientConnectedCallback += OnPlayerJoined;
        networkManager.OnClientDisconnectCallback += OnPlayerLeft;
        
        Debug.Log($"Session configured for max {maxPlayers} players");
    }
    
    /// <summary>
    /// Handle connection approval - reject if lobby is full
    /// </summary>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        
        // Check if we have room for more players
        bool approve = networkManager.ConnectedClients.Count < maxPlayers;
        
        if (approve)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null; // Use default player prefab
            
            Debug.Log($"Connection approved. Total players: {networkManager.ConnectedClients.Count + 1}/{maxPlayers}");
        }
        else
        {
            response.Approved = false;
            response.Reason = $"Session is full (max {maxPlayers} players)";
            Debug.Log($"Connection denied - session is full ({maxPlayers}/{maxPlayers})");
        }
    }
    
    private void OnPlayerJoined(ulong clientId)
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        
        Debug.Log($"Player joined. Total players: {networkManager.ConnectedClients.Count}");
        
        // Check if we have 4 players and we're the host
        if (networkManager.ConnectedClients.Count >= maxPlayers && networkManager.IsHost)
        {
            Debug.Log("Lobby full! Starting game...");
            StartGame();
        }
    }
    
    private void OnPlayerLeft(ulong clientId)
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        
        Debug.Log($"Player left. Total players: {networkManager.ConnectedClients.Count}");
    }
    
    /// <summary>
    /// Start the game and transition to gameplay scene
    /// </summary>
    private void StartGame()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsHost) return;
        
        Debug.Log("Transitioning to gameplay scene...");
        networkManager.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
    
    /// <summary>
    /// Manual method to start game (can be called by host)
    /// </summary>
    public void ManualStartGame()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager != null && networkManager.IsHost)
        {
            StartGame();
        }
    }
    
    /// <summary>
    /// Get current player count
    /// </summary>
    public int GetPlayerCount()
    {
        var networkManager = NetworkManager.Singleton;
        return networkManager != null ? networkManager.ConnectedClients.Count : 0;
    }
    
    /// <summary>
    /// Check if session is full
    /// </summary>
    public bool IsSessionFull()
    {
        return GetPlayerCount() >= maxPlayers;
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        var networkManager = NetworkManager.Singleton;
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnPlayerJoined;
            networkManager.OnClientDisconnectCallback -= OnPlayerLeft;
            networkManager.ConnectionApprovalCallback = null;
        }
    }
}