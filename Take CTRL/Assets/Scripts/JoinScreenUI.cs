using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI interactions on the Join Screen
/// Contains session code input and Join Lobby button that works with Unity Widgets
/// </summary>
public class JoinScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField sessionCodeInput;
    [SerializeField] private Button confirmButton; // This should be "Join Lobby" button
    [SerializeField] private Button backButton;
    
    [Header("Settings")]
    [SerializeField] private string placeholderText = "Enter Session Code";
    
    // Prevent infinite recursion
    private bool isProcessingJoinRequest = false;
    
    private void Start()
    {
        SetupUI();
        SetupButtons();
    }
    
    private void SetupUI()
    {
        // Set placeholder text
        if (sessionCodeInput != null)
        {
            var placeholder = sessionCodeInput.placeholder as TMP_Text;
            if (placeholder != null)
            {
                placeholder.text = placeholderText;
            }
            
            // Auto-focus the input field
            sessionCodeInput.Select();
            sessionCodeInput.ActivateInputField();
        }
        
        // Update confirm button state
        UpdateConfirmButton();
    }
    
    private void SetupButtons()
    {
        // Setup Confirm button
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
        else
        {
            Debug.LogWarning("Confirm button not assigned in JoinScreenUI");
        }
        
        // Setup Back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            Debug.LogWarning("Back button not assigned in JoinScreenUI");
        }
        
        // Listen for input field changes
        if (sessionCodeInput != null)
        {
            sessionCodeInput.onValueChanged.AddListener(OnSessionCodeChanged);
            sessionCodeInput.onEndEdit.AddListener(OnSessionCodeEndEdit);
        }
    }
    
    private void OnSessionCodeChanged(string value)
    {
        UpdateConfirmButton();
    }
    
    private void OnSessionCodeEndEdit(string value)
    {
        // Auto-confirm if user presses Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
            {
                OnConfirmButtonClicked();
            }
        }
    }
    
    private void UpdateConfirmButton()
    {
        // Enable confirm button only if session code is not empty
        if (confirmButton != null && sessionCodeInput != null)
        {
            confirmButton.interactable = !string.IsNullOrWhiteSpace(sessionCodeInput.text);
        }
    }
    
    private void OnConfirmButtonClicked()
    {
        // Prevent infinite recursion
        if (isProcessingJoinRequest)
        {
            Debug.LogWarning("Join request already in progress, ignoring duplicate click");
            return;
        }
        
        isProcessingJoinRequest = true;
        
        string sessionCode = sessionCodeInput.text.Trim();
        
        if (string.IsNullOrEmpty(sessionCode))
        {
            Debug.LogWarning("Session code is empty! Please enter a valid session code.");
            isProcessingJoinRequest = false;
            return;
        }
        
        Debug.Log($"Join Screen: Attempting to join session with code: {sessionCode}");
        
        // Store session code for later use
        SceneNavigator.PendingSessionCode = sessionCode;
        
        // Disable button to prevent multiple clicks
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        
        // Let Unity Widgets handle the networking automatically
        Debug.Log("Unity Widgets will handle session joining and networking");
        
        // Wait a moment for Unity Widgets to establish connection then navigate to lobby
        StartCoroutine(NavigateToLobbyAfterDelay());
    }
    
    /// <summary>
    /// Waits for a brief moment to allow connection to establish, then navigates to lobby
    /// </summary>
    private System.Collections.IEnumerator NavigateToLobbyAfterDelay()
    {
        Debug.Log("Waiting for connection to establish...");
        yield return new WaitForSeconds(1.0f);
        
        // Navigate to lobby scene as client
        string sessionCode = sessionCodeInput.text.Trim();
        if (SceneNavigator.Instance != null)
        {
            SceneNavigator.Instance.GoToLobbyAsClient(sessionCode);
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
            // Re-enable button and reset flag on error
            if (confirmButton != null) confirmButton.interactable = true;
            isProcessingJoinRequest = false;
        }
    }
    
    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked - returning to title");
        SceneNavigator.NavigateBack();
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        if (sessionCodeInput != null)
        {
            sessionCodeInput.onValueChanged.RemoveListener(OnSessionCodeChanged);
            sessionCodeInput.onEndEdit.RemoveListener(OnSessionCodeEndEdit);
        }
        
        // Reset processing flag
        isProcessingJoinRequest = false;
    }
}