using UnityEngine;

public class LevelBuild : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Chunk Settings")]
    [SerializeField] private GameObject[] chunkPrefabs = new GameObject[5];
    [SerializeField] private int numberOfChunks = 10;
    [SerializeField] private Vector3 chunkSpawnOffset = Vector3.right * 21f;
    [SerializeField] private float chunkYOffset = 3f;
    
    [Header("Chunk Top Prefab")]
    [SerializeField] private GameObject chunkTopPrefab;
    [SerializeField] private Vector3 topPrefabOffset = Vector3.up * 1f;
    
    [Header("Level Building")]
    [SerializeField] private bool buildOnStart = true;
    
    private Vector3 nextChunkPosition;
    
    void Start()
    {
        if (buildOnStart)
        {
            BuildLevel();
        }
    }
    
    public void BuildLevel()
    {
        SpawnChunks();
    }

    
    private void SpawnChunks()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("No chunk prefabs assigned!");
            return;
        }
        
        // Check if all chunk prefabs are assigned
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            if (chunkPrefabs[i] == null)
            {
                Debug.LogWarning($"Chunk prefab at index {i} is not assigned!");
            }
        }
        
        // Initialize starting position for chunks
        Vector3 basePosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        nextChunkPosition = new Vector3(basePosition.x + chunkSpawnOffset.x, basePosition.y + chunkYOffset, basePosition.z);
        
        // Spawn random chunks
        for (int i = 0; i < numberOfChunks; i++)
        {
            SpawnRandomChunk();
        }
        
        Debug.Log($"Spawned {numberOfChunks} random chunks");
    }
    
    private void SpawnRandomChunk()
    {
        // Get a random chunk prefab from the available ones
        GameObject[] availableChunks = System.Array.FindAll(chunkPrefabs, chunk => chunk != null);
        
        if (availableChunks.Length == 0)
        {
            Debug.LogError("No valid chunk prefabs available!");
            return;
        }
        
        int randomIndex = Random.Range(0, availableChunks.Length);
        GameObject selectedChunk = availableChunks[randomIndex];
        
        // Spawn the chunk at the next position
        GameObject spawnedChunk = Instantiate(selectedChunk, nextChunkPosition, Quaternion.identity);
        spawnedChunk.name = $"{selectedChunk.name}_Instance_{Random.Range(1000, 9999)}";
        
        // Spawn the top prefab directly on top of the chunk
        if (chunkTopPrefab != null)
        {
            Vector3 topPosition = nextChunkPosition + topPrefabOffset;
            GameObject spawnedTopPrefab = Instantiate(chunkTopPrefab, topPosition, Quaternion.identity);
            spawnedTopPrefab.name = $"{chunkTopPrefab.name}_OnChunk_{Random.Range(1000, 9999)}";
            
            Debug.Log($"Spawned top prefab: {spawnedTopPrefab.name} at {topPosition}");
        }
        else
        {
            Debug.LogWarning("Chunk top prefab is not assigned!");
        }
        
        // Update position for next chunk (only X changes, Y stays constant)
        nextChunkPosition += new Vector3(chunkSpawnOffset.x, 0, 0);
        
        Debug.Log($"Spawned chunk: {spawnedChunk.name} at {nextChunkPosition - chunkSpawnOffset}");
    }
    
    [ContextMenu("Rebuild Level")]
    public void RebuildLevel()
    {
        ClearLevel();
        BuildLevel();
    }
    
    public void ClearLevel()
    {
        // Clear existing chunks and top prefabs (find all instances)
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name.Contains("Chunk") && obj.name.Contains("Instance")) ||
                (obj.name.Contains("OnChunk")))
            {
                DestroyImmediate(obj);
            }
        }
        
        Debug.Log("Level cleared");
    }
    
    void OnValidate()
    {
        // Ensure we have exactly 5 chunk prefab slots
        if (chunkPrefabs == null || chunkPrefabs.Length != 5)
        {
            chunkPrefabs = new GameObject[5];
        }
        
        // Ensure numberOfChunks is at least 1
        if (numberOfChunks < 1)
        {
            numberOfChunks = 1;
        }
    }
}