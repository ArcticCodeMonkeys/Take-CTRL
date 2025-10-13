using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls a swarm of drones. Place this on the parent "Swarm" object.
/// All child drones with SwarmDroneScript will automatically follow this transform.
/// </summary>
public class SwarmController : MonoBehaviour
{
    [Header("Swarm Movement")]
    [SerializeField] private float moveSpeed = 2f;
    
    [Header("Drone Management")]
    [SerializeField] private List<SwarmDroneScript> drones = new List<SwarmDroneScript>();
    [SerializeField] private bool autoFindDrones = true;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (autoFindDrones)
        {
            FindAllDrones();
        }
        
        // Set this transform as the swarm center for all drones
        SetSwarmCenterForAllDrones();
        
        Debug.Log($"SwarmController initialized with {drones.Count} drones");
    }
    
    void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        // Simple movement to the right at constant speed
        Vector3 movement = Vector3.right * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }
    
    /// <summary>
    /// Find all SwarmDroneScript components in children
    /// </summary>
    public void FindAllDrones()
    {
        drones.Clear();
        SwarmDroneScript[] foundDrones = GetComponentsInChildren<SwarmDroneScript>();
        drones.AddRange(foundDrones);
        
        Debug.Log($"Found {drones.Count} drones in children");
    }
    
    /// <summary>
    /// Set this transform as the swarm center for all drones
    /// </summary>
    public void SetSwarmCenterForAllDrones()
    {
        foreach (SwarmDroneScript drone in drones)
        {
            if (drone != null)
            {
                drone.SetSwarmCenter(transform);
            }
        }
    }
    
    /// <summary>
    /// Add a drone to the swarm
    /// </summary>
    public void AddDrone(SwarmDroneScript drone)
    {
        if (drone != null && !drones.Contains(drone))
        {
            drones.Add(drone);
            drone.SetSwarmCenter(transform);
            Debug.Log($"Added drone {drone.name} to swarm");
        }
    }
    
    /// <summary>
    /// Remove a drone from the swarm
    /// </summary>
    public void RemoveDrone(SwarmDroneScript drone)
    {
        if (drones.Contains(drone))
        {
            drones.Remove(drone);
            Debug.Log($"Removed drone {drone.name} from swarm");
        }
    }
    
    /// <summary>
    /// Move the swarm to a specific position
    /// </summary>
    public void MoveSwarmTo(Vector3 targetPosition, float duration = 1f)
    {
        StartCoroutine(MoveToPosition(targetPosition, duration));
    }
    
    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
            
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    /// <summary>
    /// Get the number of drones in the swarm
    /// </summary>
    public int GetDroneCount()
    {
        return drones.Count;
    }
    
    void OnDrawGizmos()
    {
        // Draw swarm center
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw movement direction arrow
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.right * 2f);
    }
}