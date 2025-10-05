using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Handles UI interactions in the Lobby scene
/// Shows session info and start game functionality
/// No networking - pure UI management
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionCodeText;
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button copyButton;
    
    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 4;
    
    // Track current state
    private bool isHost = false;
    private string currentSessionCode = "";
    
    private void Start()
    {
        SetupUI();
        SetupButtons();
    }
    
    private void SetupUI()
    {
        // Determine if we're host or client based on SceneNavigator data
        isHost = SceneNavigator.CurrentLobbyType == "Host";
        
        // Setup initial UI state
        UpdateUIForRole();
    }
    
    private void SetupButtons()
    {
        // Setup Start Game button (only for host)
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(isHost); // Only show for host
        }
        
        // Setup Leave button
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveClicked);
        }
        
        // Setup Copy button
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(OnCopySessionCodeClicked);
            UpdateCopyButtonState();
        }
    }
    
    private void UpdateUIForRole()
    {
        if (isHost)
        {
            // Host: Show session name and generate session code
            if (sessionNameText != null)
            {
                sessionNameText.text = $"Session: {SceneNavigator.PendingSessionName ?? "Host Session"}";
            }
            
            if (sessionCodeText != null)
            {
                // Generate session code for host
                string sessionCode = GenerateSessionCode();
                sessionCodeText.text = $"Code: {sessionCode}";
                UpdateCopyButtonState();
            }
        }
        else
        {
            // Client: Show the code they entered
            if (sessionCodeText != null)
            {
                sessionCodeText.text = $"Joined: {SceneNavigator.PendingSessionCode ?? "Unknown"}";
            }
            
            if (sessionNameText != null)
            {
                sessionNameText.text = "In session";
            }
        }
    }
    
    /// <summary>
    /// Generate a simple session code for display purposes
    /// </summary>
    private string GenerateSessionCode()
    {
        if (!string.IsNullOrEmpty(currentSessionCode))
        {
            return currentSessionCode;
        }
        
        // Generate a simple 6-character alphanumeric code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        currentSessionCode = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        Debug.Log($"Generated session code: {currentSessionCode}");
        return currentSessionCode;
    }
    
    private void UpdateCopyButtonState()
    {
        if (copyButton != null)
        {
            // Enable copy button only when there's a valid session code and we're the host
            copyButton.interactable = !string.IsNullOrEmpty(currentSessionCode) && isHost;
        }
    }
    
    #region Button Event Handlers
    
    private void OnStartGameClicked()
    {
        if (!isHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }
        
        Debug.Log("Starting the game...");
        
        // Use SceneNavigator to start the game
        if (SceneNavigator.Instance != null)
        {
            SceneNavigator.Instance.StartGame();
        }
        else
        {
            Debug.LogError("SceneNavigator.Instance is null!");
        }
    }
    
    private void OnLeaveClicked()
    {
        Debug.Log("Leaving lobby...");
        
        // Return to title screen
        SceneNavigator.NavigateToTitle();
    }
    
    private void OnCopySessionCodeClicked()
    {
        if (!string.IsNullOrEmpty(currentSessionCode))
        {
            // Copy session code to clipboard
            GUIUtility.systemCopyBuffer = currentSessionCode;
            Debug.Log($"Session code '{currentSessionCode}' copied to clipboard!");
            
            // Show brief feedback
            StartCoroutine(ShowCopiedFeedback());
        }
        else
        {
            Debug.LogWarning("No session code available to copy");
        }
    }
    
    /// <summary>
    /// Show brief "Copied!" feedback to user
    /// </summary>
    private System.Collections.IEnumerator ShowCopiedFeedback()
    {
        if (sessionCodeText != null)
        {
            string originalText = sessionCodeText.text;
            sessionCodeText.text = "Copied!";
            yield return new WaitForSeconds(1f);
            sessionCodeText.text = originalText;
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
        if (leaveButton != null)
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
        if (copyButton != null)
            copyButton.onClick.RemoveListener(OnCopySessionCodeClicked);
    }
}