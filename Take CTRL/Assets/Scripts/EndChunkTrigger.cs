using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger component for the end chunk that switches all players to the win screen when entered.
/// Attach this to the end chunk prefab with a trigger collider.
/// This will automatically add a BoxCollider2D component if one doesn't exist.
/// Uses NetworkManager to synchronize scene loading across all clients.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class EndChunkTrigger : MonoBehaviour
{
    [Header("Win Screen Settings")]
    [SerializeField] private string winSceneName = "WinScreen"; // Name of the win scene to load
    [SerializeField] private float delayBeforeSwitch = 1f; // Delay before switching scenes (for celebration effect)
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool hasTriggered = false; // Prevent multiple triggers
    
    private void Awake()
    {
        // Ensure the BoxCollider2D is set to trigger
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null && !boxCollider.isTrigger)
        {
            boxCollider.isTrigger = true;
            Debug.LogWarning($"EndChunkTrigger on {gameObject.name}: BoxCollider2D was not set to trigger. Automatically setting it now.");
        }
        
        Debug.Log($"EndChunkTrigger initialized on {gameObject.name}.");
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug logging to see what's colliding
        Debug.Log($"EndChunkTrigger: Something entered! Object: {other.gameObject.name}, Tag: {other.tag}, HasTriggered: {hasTriggered}");
        
        // Only trigger once
        if (hasTriggered)
        {
            Debug.Log("EndChunkTrigger: Already triggered, ignoring.");
            return;
        }
        
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered end chunk! Triggering win condition.");
            
            hasTriggered = true;
            TriggerWinCondition();
        }
        else
        {
            Debug.LogWarning($"EndChunkTrigger: Colliding object '{other.gameObject.name}' does not have 'Player' tag!");
        }
    }
    
    /// <summary>
    /// Trigger the win condition - switches all players to the win scene
    /// </summary>
    private void TriggerWinCondition()
    {
        Debug.Log($"🎉 Player reached the end! Switching to win scene: {winSceneName}");
        
        // Start coroutine to switch scene after delay
        StartCoroutine(SwitchToWinSceneAfterDelay(delayBeforeSwitch));
    }
    
    /// <summary>
    /// Coroutine to switch to the Win scene after a delay
    /// </summary>
    private System.Collections.IEnumerator SwitchToWinSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Check if we're in a networked game
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            // In a networked game - only the server should switch scenes
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"🔄 Server switching all clients to {winSceneName} scene...");
                
                // Use NetworkManager to switch scene for all clients
                NetworkManager.Singleton.SceneManager.LoadScene(winSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log($"Client detected win - waiting for server to switch to {winSceneName}...");
                // Client detected the win, but let the server handle the scene switch
                // In a proper implementation, you might want to send a message to the server here
            }
        }
        else
        {
            // Not in a networked game - load scene locally
            Debug.Log($"Loading {winSceneName} scene locally (no network connection)");
            
            if (!string.IsNullOrEmpty(winSceneName))
            {
                SceneManager.LoadScene(winSceneName);
            }
            else
            {
                Debug.LogError("Win scene name is not set!");
            }
        }
    }
    
    /// <summary>
    /// Visual indicator in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw a green sphere to indicate this is the end chunk
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 3f);
        
        // Draw a finish line visual
        Gizmos.color = Color.yellow;
        Vector3 topPoint = transform.position + Vector3.up * 10f;
        Vector3 bottomPoint = transform.position + Vector3.down * 10f;
        Gizmos.DrawLine(topPoint, bottomPoint);
    }
}
