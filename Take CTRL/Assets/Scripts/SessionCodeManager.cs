using UnityEngine;
using TMPro;

/// <summary>
/// Manages session code display and integration with Unity Multiplayer Widgets
/// This script bridges the Unity Widgets session code with your UI elements
/// </summary>
public class SessionCodeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionCodeDisplayText; // Your custom display text
    [SerializeField] private UnityEngine.UI.Button copyButton; // Your copy button
    
    [Header("Widget References")]
    [SerializeField] private GameObject showSessionCodeWidget; // The Unity Widget "Show Session Code" object
    
    // Current session code
    private string currentSessionCode = "";
    
    // Events
    public System.Action<string> OnSessionCodeUpdated;
    
    private void Start()
    {
        // Find the Show Session Code widget if not assigned
        if (showSessionCodeWidget == null)
        {
            showSessionCodeWidget = GameObject.Find("Show Session Code");
        }
        
        // Setup copy button
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(CopySessionCodeToClipboard);
            copyButton.interactable = false; // Disabled until we have a code
        }
        
        // Start monitoring for session code
        StartCoroutine(MonitorSessionCode());
    }
    
    private System.Collections.IEnumerator MonitorSessionCode()
    {
        // Keep checking every 2 seconds for a session code
        while (string.IsNullOrEmpty(currentSessionCode))
        {
            yield return new WaitForSeconds(2f);
            CheckForSessionCode();
        }
    }
    
    private void CheckForSessionCode()
    {
        if (showSessionCodeWidget == null) return;
        
        // Look for TMP_Text components in the Show Session Code widget
        var textComponents = showSessionCodeWidget.GetComponentsInChildren<TMP_Text>();
        
        foreach (var textComponent in textComponents)
        {
            string text = textComponent.text;
            
            // Check if this looks like a valid session code
            if (!string.IsNullOrEmpty(text) && IsValidSessionCode(text))
            {
                UpdateSessionCode(text);
                return;
            }
        }
        
        // Also check regular UI Text components (in case widgets use those)
        var uiTextComponents = showSessionCodeWidget.GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (var textComponent in uiTextComponents)
        {
            string text = textComponent.text;
            
            if (!string.IsNullOrEmpty(text) && IsValidSessionCode(text))
            {
                UpdateSessionCode(text);
                return;
            }
        }
    }
    
    private bool IsValidSessionCode(string code)
    {
        // Filter out placeholder text and invalid codes
        if (string.IsNullOrEmpty(code) || 
            code.ToLower().Contains("session") || 
            code.ToLower().Contains("code") ||
            code.ToLower().Contains("join") ||
            code.Length < 4)
        {
            return false;
        }
        
        // Valid session codes are typically 6+ characters, alphanumeric
        return code.Length >= 4 && code.Length <= 30;
    }
    
    private void UpdateSessionCode(string newCode)
    {
        if (currentSessionCode != newCode)
        {
            currentSessionCode = newCode;
            
            // Update the display text
            if (sessionCodeDisplayText != null)
            {
                sessionCodeDisplayText.text = currentSessionCode;
            }
            
            // Enable copy button
            if (copyButton != null)
            {
                copyButton.interactable = true;
            }
            
            // Trigger event
            OnSessionCodeUpdated?.Invoke(currentSessionCode);
            
            Debug.Log($"SessionCodeManager: Session code updated to '{currentSessionCode}'");
        }
    }
    
    private void CopySessionCodeToClipboard()
    {
        if (!string.IsNullOrEmpty(currentSessionCode))
        {
            GUIUtility.systemCopyBuffer = currentSessionCode;
            Debug.Log($"Session code '{currentSessionCode}' copied to clipboard!");
            
            // Optional: Show a brief "Copied!" message
            StartCoroutine(ShowCopiedFeedback());
        }
        else
        {
            Debug.LogWarning("No session code available to copy");
        }
    }
    
    private System.Collections.IEnumerator ShowCopiedFeedback()
    {
        if (sessionCodeDisplayText != null)
        {
            string originalText = sessionCodeDisplayText.text;
            sessionCodeDisplayText.text = "Copied!";
            yield return new WaitForSeconds(1f);
            sessionCodeDisplayText.text = originalText;
        }
    }
    
    /// <summary>
    /// Public method to manually set session code (if needed)
    /// </summary>
    public void SetSessionCode(string code)
    {
        UpdateSessionCode(code);
    }
    
    /// <summary>
    /// Public method to get current session code
    /// </summary>
    public string GetSessionCode()
    {
        return currentSessionCode;
    }
    
    private void OnDestroy()
    {
        if (copyButton != null)
        {
            copyButton.onClick.RemoveListener(CopySessionCodeToClipboard);
        }
    }
}