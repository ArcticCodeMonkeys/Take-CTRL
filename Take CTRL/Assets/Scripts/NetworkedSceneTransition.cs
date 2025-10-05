using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles networked scene transitions for Multiplayer Widgets
/// Attach this to any GameObject in Host/Join screens
/// Call LoadLobbyScene() from widget events to transition all players simultaneously
/// </summary>
public class NetworkedSceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string lobbySceneName = "Lobby";
    
    /// <summary>
    /// Call this method from Multiplayer Widget "Joined Session ()" events
    /// This will load the lobby scene for ALL connected players simultaneously
    /// </summary>
    public void LoadLobbyScene()
    {
        StartCoroutine(WaitAndLoadLobby());
    }
    
    private System.Collections.IEnumerator WaitAndLoadLobby()
    {
        Debug.Log("Waiting for NetworkManager to be created by Multiplayer Widget...");
        
        // Wait longer for the widget to create NetworkManager
        float waitTime = 0f;
        float maxWaitTime = 10f; // Maximum 10 seconds
        
        while (NetworkManager.Singleton == null && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not created after 10 seconds! Multiplayer Widget may have failed.");
            yield break;
        }
        
        Debug.Log("NetworkManager found! Waiting for networking to establish...");
        
        // Wait a bit more for networking to fully establish
        yield return new WaitForSeconds(1f);
        
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host loading lobby scene for all players...");
            // Host loads scene for everyone - this is synchronized automatically
            NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Client waiting for host to load lobby scene...");
            // Clients automatically follow when host loads scene - no action needed
        }
        else
        {
            Debug.LogWarning("NetworkManager exists but not connected. Widget should handle this.");
            // Don't try to start hosting - let the widget handle it
        }
    }
    
    /// <summary>
    /// Alternative method if you want to load any scene (not just lobby)
    /// </summary>
    public void LoadNetworkedScene(string sceneName)
    {
        StartCoroutine(WaitAndLoadScene(sceneName));
    }
    
    private System.Collections.IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"Host loading {sceneName} for all players...");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            Debug.Log($"Client waiting for host to load {sceneName}...");
        }
        else
        {
            Debug.LogWarning($"Loading {sceneName} locally (no network connection)");
            SceneManager.LoadScene(sceneName);
        }
    }
}