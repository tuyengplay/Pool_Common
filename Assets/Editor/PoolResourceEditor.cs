using System.IO;
using UnityEditor;
using UnityEngine;

public class PoolResourceEditor : EditorWindow
{
    [MenuItem("Tools/Creat")]
    public static void CheckAndCreateDirectories()
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
}
