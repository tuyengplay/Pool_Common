using System.IO;
using UnityEditor;
using UnityEngine;

public class ResourceEditor : EditorWindow
{
    [MenuItem("Tools/Creat Prefabs")]
    public static void CheckAndCreateDirectoriesPrefabs()
    {
        string pathPrefabs = "Assets/_Project/Resources/Prefabs";
        if (!Directory.Exists(pathPrefabs))
        {
            Directory.CreateDirectory(pathPrefabs);
            Debug.Log("Created Directory: " + pathPrefabs);
        }
        else
        {
            Debug.Log("Directory already exists: " + pathPrefabs);

        }
        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/Creat Sprites")]
    public static void CheckAndCreateDirectoriesSprites()
    {
        string pathPrefabs = "Assets/_Project/Resources/Sprites";
        if (!Directory.Exists(pathPrefabs))
        {
            Directory.CreateDirectory(pathPrefabs);
            Debug.Log("Created Directory: " + pathPrefabs);
        }
        else
        {
            Debug.Log("Directory already exists: " + pathPrefabs);

        }
        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/Creat Audio")]
    public static void CheckAndCreateDirectoriesAudio()
    {
        string pathPrefabs = "Assets/_Project/Resources/Audio";
        if (!Directory.Exists(pathPrefabs))
        {
            Directory.CreateDirectory(pathPrefabs);
            Debug.Log("Created Directory: " + pathPrefabs);
        }
        else
        {
            Debug.Log("Directory already exists: " + pathPrefabs);

        }
        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/Creat Data")]
    public static void CheckAndCreateDirectoriesData()
    {
        string pathPrefabs = "Assets/_Project/Resources/Data";
        if (!Directory.Exists(pathPrefabs))
        {
            Directory.CreateDirectory(pathPrefabs);
            Debug.Log("Created Directory: " + pathPrefabs);
        }
        else
        {
            Debug.Log("Directory already exists: " + pathPrefabs);

        }
        AssetDatabase.Refresh();
    }
}
