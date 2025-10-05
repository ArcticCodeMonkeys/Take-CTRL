using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles all scene transitions and navigation flow
/// Manages the progression from Title to Host/Join to Lobby to Gameplay
/// </summary>
public class SceneNavigator : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title Screen";
    [SerializeField] private string hostSceneName = "Host Screen";
    [SerializeField] private string joinSceneName = "Join Screen";
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string gameplaySceneName = "Warehouse";
    [SerializeField] private string winSceneName = "Win Screen";
    [SerializeField] private string loseSceneName = "Lose Screen";
    
    public static SceneNavigator Instance { get; private set; }
    
    // Current scene state
    public static string CurrentLobbyType { get; private set; } // "Host" or "Join"
    public static string PendingSessionName { get; set; } // For host
    public static string PendingSessionCode { get; set; } // For join
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ SceneNavigator Instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #region Public Navigation Methods
    
    /// <summary>
    /// Navigate to Host screen from Title
    /// </summary>
    public void GoToHostScreen()
    {
        Debug.Log("Navigating to Host Screen");
        CurrentLobbyType = "Host";
        LoadScene(hostSceneName);
    }
    
    /// <summary>
    /// Navigate to Join screen from Title
    /// </summary>
    public void GoToJoinScreen()
    {
        Debug.Log("Navigating to Join Screen");
        CurrentLobbyType = "Join";
        LoadScene(joinSceneName);
    }
    
    /// <summary>
    /// Navigate to Lobby from Host screen (with session name)
    /// </summary>
    public void GoToLobbyAsHost(string sessionName)
    {
        Debug.Log($"🎯 GoToLobbyAsHost called with session name: '{sessionName}'");
        Debug.Log($"📍 Current scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"🎬 Target lobby scene: '{lobbySceneName}'");
        
        PendingSessionName = sessionName;
        CurrentLobbyType = "Host";
        
        Debug.Log($"📝 Set PendingSessionName to: '{PendingSessionName}'");
        Debug.Log($"📝 Set CurrentLobbyType to: '{CurrentLobbyType}'");
        
        LoadScene(lobbySceneName);
    }
    
    /// <summary>
    /// Navigate to Lobby from Join screen (with session code)
    /// </summary>
    public void GoToLobbyAsClient(string sessionCode)
    {
        Debug.Log($"Going to Lobby as Client with session code: {sessionCode}");
        PendingSessionCode = sessionCode;
        CurrentLobbyType = "Join";
        LoadScene(lobbySceneName);
    }
    
    /// <summary>
    /// Start the game - transition to gameplay scene
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting game - transitioning to gameplay");
        LoadScene(gameplaySceneName);
    }
    
    /// <summary>
    /// Return to Title screen (clears all networking)
    /// </summary>
    public void ReturnToTitle()
    {
        Debug.Log("Returning to Title Screen");
        
        // Clear pending data
        PendingSessionName = null;
        PendingSessionCode = null;
        CurrentLobbyType = null;
        
        LoadScene(titleSceneName);
    }
    
    /// <summary>
    /// Go back from Host/Join to Title (static method for UI buttons)
    /// </summary>
    public static void NavigateBack()
    {
        if (Instance != null)
        {
            Instance.ReturnToTitle();
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
        }
    }
    
    /// <summary>
    /// Navigate to Host screen (static method for UI buttons)
    /// </summary>
    public static void NavigateToHostScreen()
    {
        if (Instance != null)
        {
            Instance.GoToHostScreen();
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
        }
    }
    
    /// <summary>
    /// Navigate to Join screen (static method for UI buttons)
    /// </summary>
    public static void NavigateToJoinScreen()
    {
        if (Instance != null)
        {
            Instance.GoToJoinScreen();
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
        }
    }
    
    /// <summary>
    /// Navigate to Title screen (static method for UI buttons)
    /// </summary>
    public static void NavigateToTitle()
    {
        if (Instance != null)
        {
            Instance.ReturnToTitle();
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Load scene by name with extensive debugging
    /// </summary>
    private void LoadScene(string sceneName)
    {
        Debug.Log($"🚀 LoadScene called with sceneName: '{sceneName}'");
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name is null or empty!");
            return;
        }
        
        // Check if the scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameFromPath == sceneName)
            {
                sceneExists = true;
                Debug.Log($"✅ Scene '{sceneName}' found in build settings at index {i}");
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"❌ Scene '{sceneName}' not found in build settings!");
            Debug.LogError("Make sure the scene is added to File → Build Settings → Scenes in Build");
            
            // List all available scenes
            Debug.Log("Available scenes in build settings:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string availableSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"  [{i}] {availableSceneName}");
            }
            return;
        }
        
        Debug.Log($"🎬 Loading scene: '{sceneName}'");
        
        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"✅ SceneManager.LoadScene('{sceneName}') called successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception loading scene '{sceneName}': {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
    
    #endregion
}