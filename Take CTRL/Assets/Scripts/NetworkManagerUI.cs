using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button backButton;
    [SerializeField] private InputField ipInputField; // Add this field in Inspector
    
    [Header("Robot Settings")]
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private static bool robotSpawned = false;
    private Coroutine connectionTimeoutCoroutine;

    private void Awake()
    {
        // Add null checks to prevent NullReferenceException
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClicked);
        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientClicked);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
            
        // Set default IP to localhost
        if (ipInputField != null)
            ipInputField.text = "127.0.0.1";
    }

    private void OnHostClicked()
    {
        // Handle host button click
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Starting as Host - Shared robot will be spawned");
            
            // Spawn robot after becoming host
            if (NetworkManager.Singleton.IsHost && !robotSpawned)
            {
                StartCoroutine(SpawnRobotAfterDelay());
            }
        }
    }
    
    private System.Collections.IEnumerator SpawnRobotAfterDelay()
    {
        // Wait a frame to ensure host is fully started
        yield return null;
        
        if (robotPrefab != null && NetworkManager.Singleton.IsHost)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            GameObject robotObj = Instantiate(robotPrefab, spawnPosition, Quaternion.identity);
            
            NetworkObject networkObject = robotObj.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
                robotSpawned = true;
                Debug.Log("Shared robot spawned successfully!");
            }
            else
            {
                Debug.LogError("Robot prefab must have a NetworkObject component!");
                Destroy(robotObj);
            }
        }
        else
        {
            Debug.LogError("Robot prefab is not assigned!");
        }
    }

    private void OnClientClicked()
    {
        // Handle client button click
        if (NetworkManager.Singleton != null)
        {
            // Get IP from input field
            string targetIP = "127.0.0.1"; // Default to localhost
            if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
            {
                targetIP = ipInputField.text.Trim();
            }
            
            Debug.Log($"Client button clicked. Target IP: {targetIP}");
            
            // Set the connection data
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = targetIP;
                transport.ConnectionData.Port = 7778; // Make sure port matches
                Debug.Log($"Transport configured - Address: {targetIP}, Port: 7778");
            }
            else
            {
                Debug.LogError("UnityTransport component not found!");
                return;
            }
            
            // Subscribe to connection events for debugging
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            // Start connection timeout
            if (connectionTimeoutCoroutine != null)
                StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeout(10f)); // 10 second timeout
            
            NetworkManager.Singleton.StartClient();
            Debug.Log("StartClient() called - attempting connection...");
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null!");
        }
    }
    
    private IEnumerator ConnectionTimeout(float timeoutSeconds)
    {
        float timer = 0f;
        while (timer < timeoutSeconds)
        {
            timer += Time.deltaTime;
            
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Connection successful!");
                yield break; // Connection successful, exit
            }
            
            yield return null;
        }
        
        // Timeout reached
        Debug.LogError($"Connection timeout after {timeoutSeconds} seconds. Check:");
        Debug.LogError("1. Host is running and listening");
        Debug.LogError("2. IP address is correct");
        Debug.LogError("3. Port 7778 is not blocked by firewall");
        Debug.LogError("4. Both machines are on same network");
        
        // Try to shutdown and cleanup
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client connected successfully! ClientId: {clientId}");
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client disconnected! ClientId: {clientId}");
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
    }

    private void OnBackClicked()
    {
        // Handle back button click

    }
}
