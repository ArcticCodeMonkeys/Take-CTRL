using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject robot;
    public float verticalOffset = 0;
    public float horizontalOffset = 0;
    public float followSpeed = 2f; // Controls how fast the camera follows (lower = more lag)
    
    private float fixedYPosition; // Store the Y position we want to maintain
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the fixed Y position based on initial robot position + offset
        if (robot != null)
        {
            fixedYPosition = robot.transform.position.y + verticalOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (robot != null)
        {
            // Calculate target X position (with horizontal offset)
            float targetX = robot.transform.position.x + horizontalOffset;
            
            // Smoothly move towards the target X position using Lerp
            float currentX = Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime);
            
            // Keep Y position fixed and Z position for 2D camera
            transform.position = new Vector3(currentX, fixedYPosition, transform.position.z);
        }
    }
}
