using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

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
            
            // Set the connection data
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = targetIP;
                Debug.Log($"Connecting to: {targetIP}");
            }
            
            NetworkManager.Singleton.StartClient();
            Debug.Log("Starting as Client - Will connect to shared robot");
        }
    }

    private void OnBackClicked()
    {
        // Handle back button click

    }
}
