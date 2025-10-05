using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles UI interactions on the Title Screen
/// Contains Host and Join buttons
/// </summary>
public class TitleScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton; // Optional
    
    private void Start()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        // Setup Host button
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostButtonClicked);
        }
        else
        {
            Debug.LogWarning("Host button not assigned in TitleScreenUI");
        }
        
        // Setup Join button
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
        }
        else
        {
            Debug.LogWarning("Join button not assigned in TitleScreenUI");
        }
        
        // Setup Quit button (optional)
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }
    
    private void OnHostButtonClicked()
    {
        Debug.Log("Host button clicked");
        SceneNavigator.NavigateToHostScreen();
    }
    
    private void OnJoinButtonClicked()
    {
        Debug.Log("Join button clicked");
        SceneNavigator.NavigateToJoinScreen();
    }
    
    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnHostButtonClicked);
        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }
}