using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Temporary script to verify your NGO setup is correct
/// Attach this to any GameObject to run diagnostics
/// Remove this script once everything is working
/// </summary>
public class NGOSetupChecker : MonoBehaviour
{
    [Header("Check Results (Read Only)")]
    [SerializeField] private bool networkManagerFound;
    [SerializeField] private bool sessionManagerFound;
    [SerializeField] private bool robotManagerFound;
    [SerializeField] private bool defaultNetworkPrefabsFound;
    [SerializeField] private bool multiplayerWidgetsFound;
    
    private void Start()
    {
        RunSetupCheck();
    }
    
    [ContextMenu("Run Setup Check")]
    public void RunSetupCheck()
    {
        Debug.Log("=== NGO Setup Check ===");
        
        // Check for NetworkManager
        var networkManager = FindObjectOfType<NetworkManager>();
        networkManagerFound = networkManager != null;
        Debug.Log($"✓ NetworkManager: {(networkManagerFound ? "FOUND" : "MISSING")}");
        
        if (networkManagerFound)
        {
            Debug.Log($"  - Connection Approval: {(networkManager.NetworkConfig.ConnectionApproval ? "ENABLED" : "DISABLED")}");
            Debug.Log($"  - Scene Management: {(networkManager.NetworkConfig.EnableSceneManagement ? "ENABLED" : "DISABLED")}");
        }
        
        // Check for SessionManager
        var sessionManager = FindObjectOfType<SessionManager>();
        sessionManagerFound = sessionManager != null;
        Debug.Log($"✓ SessionManager: {(sessionManagerFound ? "FOUND" : "MISSING")}");
        
        // Check for RobotManager
        var robotManager = FindObjectOfType<RobotManager>();
        robotManagerFound = robotManager != null;
        Debug.Log($"✓ RobotManager: {(robotManagerFound ? "FOUND" : "MISSING")}");
        
        // Check for DefaultNetworkPrefabs
        var prefabsList = Resources.Load("DefaultNetworkPrefabs");
        defaultNetworkPrefabsFound = prefabsList != null;
        Debug.Log($"✓ DefaultNetworkPrefabs: {(defaultNetworkPrefabsFound ? "FOUND" : "MISSING")}");
        
        // Check for Multiplayer Widgets (basic check)
        var widgets = FindObjectsOfType<MonoBehaviour>();
        multiplayerWidgetsFound = false;
        foreach (var widget in widgets)
        {
            if (widget.GetType().Name.Contains("Session") || widget.GetType().Name.Contains("Multiplayer"))
            {
                multiplayerWidgetsFound = true;
                Debug.Log($"  - Found widget: {widget.GetType().Name}");
            }
        }
        Debug.Log($"✓ Multiplayer Widgets: {(multiplayerWidgetsFound ? "FOUND" : "MISSING")}");
        
        // Scene-specific checks
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"Current Scene: {sceneName}");
        
        switch (sceneName)
        {
            case "Host Screen":
            case "Join Screen":
                Debug.Log("  Expected: NetworkManager + SessionManager + Multiplayer Widgets");
                break;
            case "Lobby":
            case "Warehouse":
                Debug.Log("  Expected: NetworkManager + SessionManager + RobotManager");
                break;
            default:
                Debug.Log("  This scene may not need networking components");
                break;
        }
        
        Debug.Log("=== End Setup Check ===");
    }
}