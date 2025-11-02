using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages endless scrolling background and floor for side-scrolling levels.
/// Spawns random chunks (with enemies and obstacles) ahead of the player and removes chunks that fall behind the DroneSwarm.
/// </summary>
public class EndlessBackgroundScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player; // Robot/player for spawning chunks ahead
    [SerializeField] private Transform droneSwarm; // DroneSwarm for despawning chunks behind
    [SerializeField] private GameObject[] chunkPrefabs; // Array of chunk prefabs (Background+floor+enemies+obstacles)
    [SerializeField] private GameObject breakPrefab; // Empty chunk that spawns before each content chunk
    
    [Header("Spawn Settings")]
    [SerializeField] private float chunkWidth = 19.2f; // Width of each Background+floor chunk
    [SerializeField] private int initialChunks = 5; // Number of chunks to spawn at start
    [SerializeField] private float spawnDistance = 50f; // Distance ahead of player to spawn new chunks
    
    [Header("Cleanup Settings")]
    [SerializeField] private float deleteDistance = 100f; // Delete chunks when they're this far behind DroneSwarm
    
    // Track spawned chunks
    private List<GameObject> spawnedChunks = new List<GameObject>();
    private float nextSpawnX;
    private bool spawnBreakNext = true; // Track whether to spawn a break chunk or content chunk next
    
    void Start()
    {
        if (spawnPoint == null)
        {
            GameObject spawnObject = GameObject.Find("SpawnPoint");
            if (spawnObject != null)
            {
                spawnPoint = spawnObject.transform;
            }
            else
            {
                Debug.LogWarning("EndlessBackgroundScroller: SpawnPoint not found! Using world origin.");
                GameObject tempSpawn = new GameObject("TempSpawnPoint");
                spawnPoint = tempSpawn.transform;
                spawnPoint.position = Vector3.zero;
            }
        }
        
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("EndlessBackgroundScroller: No chunk prefabs assigned!");
            enabled = false;
            return;
        }
        
        if (breakPrefab == null)
        {
            Debug.LogWarning("EndlessBackgroundScroller: Break prefab not assigned! Will only spawn content chunks.");
        }
        
        // Check if any chunk prefabs are assigned
        bool hasValidPrefab = false;
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            if (chunkPrefabs[i] != null)
            {
                hasValidPrefab = true;
                break;
            }
        }
        
        if (!hasValidPrefab)
        {
            Debug.LogError("EndlessBackgroundScroller: No valid chunk prefabs found in array!");
            enabled = false;
            return;
        }
        
        // Initialize spawn position
        nextSpawnX = spawnPoint.position.x;
        
        // Spawn initial chunks
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnChunk();
        }
    }
    
    void Update()
    {
        // Check if player reference is a prefab (not in scene) - if so, clear it
        if (player != null && player.gameObject.scene.name == null)
        {
            player = null;
        }
        
        // Find player if not assigned (handles dynamic spawning)
        if (player == null)
        {
            TryFindPlayer();
            if (player == null) return;
        }
        
        // Check if droneSwarm reference is a prefab - if so, clear it
        if (droneSwarm != null && droneSwarm.gameObject.scene.name == null)
        {
            droneSwarm = null;
        }
        
        // Find droneSwarm if not assigned
        if (droneSwarm == null)
        {
            TryFindDroneSwarm();
            if (droneSwarm == null) return;
        }
        
        // Get player position for spawning new chunks
        float playerX = player.position.x;
        
        // Get DroneSwarm position for cleanup
        float droneX = droneSwarm.position.x;
        
        // Spawn new chunks if player is approaching the last spawned chunk
        while (nextSpawnX < playerX + spawnDistance)
        {
            SpawnChunk();
        }
        
        // Clean up old chunks that are too far behind DroneSwarm
        CleanupOldChunks(droneX);
    }
    
    /// <summary>
    /// Try to find the player in the scene (handles dynamic spawning)
    /// </summary>
    private void TryFindPlayer()
    {
        try
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        catch
        {
            // Player tag doesn't exist
        }
    }
    
    /// <summary>
    /// Try to find the DroneSwarm in the scene
    /// </summary>
    private void TryFindDroneSwarm()
    {
        GameObject swarmObject = GameObject.Find("DroneSwarm");
        if (swarmObject != null)
        {
            droneSwarm = swarmObject.transform;
        }
    }
    
    /// <summary>
    /// Spawn a new chunk at the next spawn position, alternating between break chunks and content chunks
    /// </summary>
    private void SpawnChunk()
    {
        GameObject selectedChunk = null;
        
        // Alternate between break chunk and content chunk
        if (spawnBreakNext && breakPrefab != null)
        {
            // Spawn a break (empty) chunk
            selectedChunk = breakPrefab;
            spawnBreakNext = false; // Next spawn will be content
        }
        else
        {
            // Spawn a content chunk (with enemies/obstacles)
            GameObject[] availableChunks = System.Array.FindAll(chunkPrefabs, chunk => chunk != null);
            
            if (availableChunks.Length == 0)
            {
                Debug.LogError("No valid chunk prefabs available!");
                return;
            }
            
            int randomIndex = Random.Range(0, availableChunks.Length);
            selectedChunk = availableChunks[randomIndex];
            spawnBreakNext = true; // Next spawn will be break
        }
        
        if (selectedChunk == null)
        {
            Debug.LogError("Selected chunk is null!");
            return;
        }
        
        Vector3 spawnPosition = new Vector3(
            nextSpawnX,
            spawnPoint.position.y + 5.05f,
            spawnPoint.position.z
        );
        
        GameObject newChunk = Instantiate(selectedChunk, spawnPosition, Quaternion.identity);
        newChunk.name = $"{selectedChunk.name}_Chunk_{spawnedChunks.Count}";
        newChunk.transform.parent = transform; // Parent to this manager for organization
        
        spawnedChunks.Add(newChunk);
        
        // Move spawn position for next chunk
        nextSpawnX += chunkWidth;
    }
    
    /// <summary>
    /// Remove chunks that are too far behind the DroneSwarm
    /// </summary>
    private void CleanupOldChunks(float droneX)
    {
        for (int i = spawnedChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = spawnedChunks[i];
            
            if (chunk == null)
            {
                spawnedChunks.RemoveAt(i);
                continue;
            }
            
            // Delete if chunk is too far behind DroneSwarm
            float chunkX = chunk.transform.position.x;
            if (chunkX < droneX - deleteDistance)
            {
                spawnedChunks.RemoveAt(i);
                Destroy(chunk);
            }
        }
    }
    
    /// <summary>
    /// Get the number of currently active chunks (useful for debugging)
    /// </summary>
    public int GetActiveChunkCount()
    {
        // Clean up null entries
        spawnedChunks.RemoveAll(chunk => chunk == null);
        return spawnedChunks.Count;
    }
    
    /// <summary>
    /// Force spawn a specific number of additional chunks (useful for testing)
    /// </summary>
    [ContextMenu("Spawn Additional Chunk")]
    public void SpawnAdditionalChunk()
    {
        SpawnChunk();
    }
    
    /// <summary>
    /// Clear all spawned chunks (useful for level reset)
    /// </summary>
    [ContextMenu("Clear All Chunks")]
    public void ClearAllChunks()
    {
        foreach (GameObject chunk in spawnedChunks)
        {
            if (chunk != null)
            {
                Destroy(chunk);
            }
        }
        spawnedChunks.Clear();
        nextSpawnX = spawnPoint.position.x;
        spawnBreakNext = true; // Reset to start with break chunk
    }
    
    // Draw gizmos in editor to visualize spawn and delete distances
    void OnDrawGizmosSelected()
    {
        if (player == null || droneSwarm == null) return;
        
        Vector3 playerPos = player.position;
        Vector3 dronePos = droneSwarm.position;
        
        // Draw spawn distance (green) - based on player position
        Gizmos.color = Color.green;
        Vector3 spawnLine = new Vector3(playerPos.x + spawnDistance, playerPos.y, playerPos.z);
        Gizmos.DrawLine(spawnLine + Vector3.up * 10, spawnLine + Vector3.down * 10);
        Gizmos.DrawWireSphere(spawnLine, 2f);
        
        // Draw delete distance (red) - based on DroneSwarm position
        Gizmos.color = Color.red;
        Vector3 deleteLine = new Vector3(dronePos.x - deleteDistance, dronePos.y, dronePos.z);
        Gizmos.DrawLine(deleteLine + Vector3.up * 10, deleteLine + Vector3.down * 10);
        Gizmos.DrawWireSphere(deleteLine, 2f);
        
        // Draw Player position (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerPos, 3f);
        
        // Draw DroneSwarm position (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(dronePos, 3f);
    }
}
