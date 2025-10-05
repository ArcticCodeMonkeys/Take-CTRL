using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI interactions on the Host Screen
/// Contains session name input and Create Lobby button that works with Unity Widgets
/// </summary>
public class HostScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private Button confirmButton; // This should be "Create Lobby" button
    [SerializeField] private Button backButton;
    
    [Header("Settings")]
    [SerializeField] private string defaultSessionName = "My Game Session";
    
    // Prevent infinite recursion
    private bool isProcessingSessionCreation = false;
    
    private void Start()
    {
        SetupUI();
        SetupButtons();
    }
    
    private void SetupUI()
    {
        // Set default session name
        if (sessionNameInput != null)
        {
            sessionNameInput.text = defaultSessionName;
            
            // Auto-select text for easy editing
            sessionNameInput.Select();
            sessionNameInput.ActivateInputField();
        }
        
        // Update confirm button state based on input
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
            Debug.LogWarning("Confirm button not assigned in HostScreenUI");
        }
        
        // Setup Back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            Debug.LogWarning("Back button not assigned in HostScreenUI");
        }
        
        // Listen for input field changes
        if (sessionNameInput != null)
        {
            sessionNameInput.onValueChanged.AddListener(OnSessionNameChanged);
        }
    }
    
    private void OnSessionNameChanged(string value)
    {
        UpdateConfirmButton();
    }
    
    private void UpdateConfirmButton()
    {
        // Enable confirm button only if session name is not empty
        if (confirmButton != null && sessionNameInput != null)
        {
            confirmButton.interactable = !string.IsNullOrWhiteSpace(sessionNameInput.text);
        }
    }
    
    private void OnConfirmButtonClicked()
    {
        // Prevent infinite recursion
        if (isProcessingSessionCreation)
        {
            Debug.LogWarning("Session creation already in progress, ignoring duplicate click");
            return;
        }
        
        isProcessingSessionCreation = true;
        
        Debug.Log("Creating lobby...");
        
        string sessionName = sessionNameInput?.text ?? defaultSessionName;
        
        // Validate session name
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            Debug.LogWarning("Session name is empty!");
            isProcessingSessionCreation = false;
            return;
        }
        
        Debug.Log($"Creating lobby with name: {sessionName}");
        
        // Store session name for later use
        SceneNavigator.PendingSessionName = sessionName;
        
        // Navigate directly to lobby
        if (SceneNavigator.Instance != null)
        {
            SceneNavigator.Instance.GoToLobbyAsHost(sessionName);
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
            isProcessingSessionCreation = false;
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
        if (sessionNameInput != null)
            sessionNameInput.onValueChanged.RemoveListener(OnSessionNameChanged);
        
        // Reset processing flag
        isProcessingSessionCreation = false;
    }
}