using System.Collections;
using UnityEngine;

/// <summary>
/// Helper MonoBehaviour for restoring save data after scene load
/// This is created temporarily to run the coroutine
/// </summary>
public class SaveRestoreHelper : MonoBehaviour
{
    public void StartRestore(SaveData saveData)
    {
        StartCoroutine(RestoreDataNextFrame(saveData));
    }

    private IEnumerator RestoreDataNextFrame(SaveData data)
    {
        // Wait one frame to ensure all scene objects are initialized
        yield return null;

        // Restore the save data
        EnhancedGameSaveManager.RestoreSaveData(data);

        Debug.Log("[SaveRestoreHelper] Game state restored successfully!");

        // Destroy this temporary helper object
        Destroy(gameObject);
    }
}
