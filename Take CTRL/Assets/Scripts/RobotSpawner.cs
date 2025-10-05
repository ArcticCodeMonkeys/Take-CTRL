using UnityEngine;
using Unity.Netcode;

public class RobotSpawner : NetworkBehaviour
{
    [Header("Robot Settings")]
    public GameObject robotPrefab;
    public Transform spawnPoint;
    
    private static bool robotSpawned = false;
    
    private void Start()
    {
        Debug.Log($"RobotSpawner Start called. IsServer: {IsServer}, IsClient: {IsClient}");
    }
    
    public override void OnNetworkSpawn()
    {
        Debug.Log($"RobotSpawner OnNetworkSpawn called. IsServer: {IsServer}, robotSpawned: {robotSpawned}");
        
        // Only spawn the robot once when the server starts
        if (IsServer && !robotSpawned)
        {
            Debug.Log("Attempting to spawn shared robot...");
            SpawnSharedRobot();
            robotSpawned = true;
        }
        else
        {
            Debug.Log($"Not spawning robot. IsServer: {IsServer}, robotSpawned: {robotSpawned}");
        }
    }
    
    private void SpawnSharedRobot()
    {
        if (robotPrefab == null)
        {
            Debug.LogError("Robot prefab is not assigned!");
            return;
        }
        
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject robotObj = Instantiate(robotPrefab, spawnPosition, Quaternion.identity);
        
        // Ensure the robot is active
        robotObj.SetActive(true);
        
        NetworkObject networkObject = robotObj.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            Debug.Log($"Shared robot spawned successfully at position: {robotObj.transform.position}");
            Debug.Log($"Robot name: {robotObj.name}, Active: {robotObj.activeInHierarchy}");
            
            // Check for camera
            Camera robotCamera = robotObj.GetComponentInChildren<Camera>();
            if (robotCamera != null)
            {
                Debug.Log($"Robot camera found: {robotCamera.name}, Active: {robotCamera.gameObject.activeInHierarchy}");
                // Ensure camera is enabled
                robotCamera.enabled = true;
                robotCamera.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("No camera found in robot prefab!");
            }
        }
        else
        {
            Debug.LogError("Robot prefab must have a NetworkObject component!");
            Destroy(robotObj);
        }
    }
}