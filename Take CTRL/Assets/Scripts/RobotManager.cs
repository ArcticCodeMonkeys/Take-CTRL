using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Manages the shared robot spawning and lifecycle using proper NGO events
/// Replaces the problematic manual NetworkManagerUI approach
/// </summary>
public class RobotManager : NetworkBehaviour
{
    [Header("Robot Settings")]
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private static bool robotSpawned = false;
    private static GameObject spawnedRobot;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"🤖 RobotManager.OnNetworkSpawn() - IsServer: {IsServer}, robotSpawned: {robotSpawned}");
        
        // Only the server should spawn the robot
        if (IsServer && !robotSpawned)
        {
            Debug.Log("🤖 Server attempting to spawn robot...");
            SpawnSharedRobot();
        }
        else
        {
            Debug.Log($"🤖 Not spawning robot - IsServer: {IsServer}, robotSpawned: {robotSpawned}");
        }
        
        // Subscribe to connection events
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up event subscriptions
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void SpawnSharedRobot()
    {
        Debug.Log("🤖 SpawnSharedRobot() called");
        
        if (robotPrefab == null)
        {
            Debug.LogError("🚨 Robot prefab is not assigned in RobotManager!");
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Debug.Log($"🤖 Spawning robot at position: {spawnPosition}");
        
        spawnedRobot = Instantiate(robotPrefab, spawnPosition, Quaternion.identity);
        
        NetworkObject networkObject = spawnedRobot.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            robotSpawned = true;
            Debug.Log("✅ Shared robot spawned successfully via RobotManager!");
        }
        else
        {
            Debug.LogError("🚨 Robot prefab must have a NetworkObject component!");
            Destroy(spawnedRobot);
            spawnedRobot = null;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected. Total clients: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        // You can add logic here for when players join
        // For example, updating UI or checking if lobby is full
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected. Total clients: {NetworkManager.Singleton.ConnectedClients.Count}");
        
        // Handle player disconnection
        // You might want to pause the game or handle reconnection logic
    }

    /// <summary>
    /// Call this to reset the robot spawning state (useful for scene transitions)
    /// </summary>
    public static void ResetRobotState()
    {
        robotSpawned = false;
        spawnedRobot = null;
    }

    public override void OnDestroy()
    {
        // Clean up static references when the manager is destroyed
        if (spawnedRobot == this.gameObject)
        {
            ResetRobotState();
        }
        
        base.OnDestroy();
    }
}