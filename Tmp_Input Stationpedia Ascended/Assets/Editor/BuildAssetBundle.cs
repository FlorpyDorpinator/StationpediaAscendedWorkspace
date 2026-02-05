using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Unity Editor script to build the SA_TMPInputField prefab into an AssetBundle.
/// 
/// Usage:
/// 1. Open Unity project "Tmp_Input Stationpedia Ascended"
/// 2. Go to menu: Assets > Build StationpediaAscended AssetBundle
/// 3. The bundle will be created at: Assets/AssetBundles/stationpediaascended_ui
/// 4. Copy the bundle file to your BepInEx mod folder
/// </summary>
public class BuildAssetBundle
{
    private const string BUNDLE_NAME = "stationpediaascended_ui";
    private const string OUTPUT_PATH = "Assets/AssetBundles";
    
    [MenuItem("Assets/Build StationpediaAscended AssetBundle")]
    public static void BuildBundle()
    {
        // Ensure output directory exists
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }
        
        // Set the prefab's AssetBundle name
        string prefabPath = "Assets/Prefabs/UI/SA_TMPInputField_Base.prefab";
        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = BUNDLE_NAME;
            Debug.Log($"Set AssetBundle name for {prefabPath}");
        }
        else
        {
            Debug.LogError($"Could not find prefab at {prefabPath}");
            return;
        }
        
        // Build the AssetBundle for Windows (Stationeers target platform)
        BuildPipeline.BuildAssetBundles(
            OUTPUT_PATH,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );
        
        Debug.Log($"AssetBundle built successfully at: {OUTPUT_PATH}/{BUNDLE_NAME}");
        
        // Also log the full path for easy copying
        string fullPath = Path.GetFullPath(Path.Combine(OUTPUT_PATH, BUNDLE_NAME));
        Debug.Log($"Full path: {fullPath}");
        
        // Refresh the asset database to show the new files
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Assets/Build StationpediaAscended AssetBundle (All Platforms)")]
    public static void BuildBundleAllPlatforms()
    {
        // Ensure output directory exists
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }
        
        // Set the prefab's AssetBundle name
        string prefabPath = "Assets/Prefabs/UI/SA_TMPInputField_Base.prefab";
        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = BUNDLE_NAME;
        }
        
        // Build for Windows
        BuildPipeline.BuildAssetBundles(
            OUTPUT_PATH + "/Windows",
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );
        
        // Build for Linux (Steam Deck, etc.)
        BuildPipeline.BuildAssetBundles(
            OUTPUT_PATH + "/Linux",
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneLinux64
        );
        
        Debug.Log("AssetBundles built for all platforms!");
        AssetDatabase.Refresh();
    }
}
