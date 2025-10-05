using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Prevents infinite recursion crashes during NetworkManager startup
/// by temporarily disabling auto-spawning of player prefabs during lobby creation
/// </summary>
public class NGOCrashPrevention : MonoBehaviour
{
    private static bool originalAutoSpawnSetting;
    private static bool hasStoredOriginalSetting = false;

    /// <summary>
    /// Call this before starting the NetworkManager to prevent crashes
    /// </summary>
    public static void PreventCrashOnStartup()
    {
        if (NetworkManager.Singleton != null)
        {
            // Store the original setting if we haven't already
            if (!hasStoredOriginalSetting)
            {
                originalAutoSpawnSetting = NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide;
                hasStoredOriginalSetting = true;
                Debug.Log($"NGOCrashPrevention: Stored original AutoSpawn setting: {originalAutoSpawnSetting}");
            }

            // Temporarily disable auto-spawning to prevent infinite recursion
            NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;
            Debug.Log("NGOCrashPrevention: Disabled auto-spawning to prevent crashes");
        }
        else
        {
            Debug.LogWarning("NGOCrashPrevention: NetworkManager.Singleton is null!");
        }
    }

    /// <summary>
    /// Call this when you want to restore normal player spawning (e.g., when entering gameplay)
    /// </summary>
    public static void RestoreNormalSpawning()
    {
        if (NetworkManager.Singleton != null && hasStoredOriginalSetting)
        {
            NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = originalAutoSpawnSetting;
            Debug.Log($"NGOCrashPrevention: Restored original AutoSpawn setting: {originalAutoSpawnSetting}");
        }
        else
        {
            Debug.LogWarning("NGOCrashPrevention: Cannot restore - NetworkManager is null or no setting stored");
        }
    }

    /// <summary>
    /// Emergency reset in case something goes wrong
    /// </summary>
    public static void Reset()
    {
        hasStoredOriginalSetting = false;
        Debug.Log("NGOCrashPrevention: Reset stored settings");
    }
}