using UnityEngine;
using UnityEditor;

public class RenameProject : Editor
{
    [MenuItem("Tools/Rename Project to SixStones")]
    public static void RenameToSixStones()
    {
        PlayerSettings.productName = "SixStones";
        PlayerSettings.companyName = "Your Company"; // Change this if needed
        
        Debug.Log("Project renamed to: SixStones");
        Debug.Log("Company name set to: " + PlayerSettings.companyName);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
