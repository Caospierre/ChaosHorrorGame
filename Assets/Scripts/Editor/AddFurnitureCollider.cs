using UnityEditor;
using UnityEngine;
using System.IO;

public class AddFurnitureCollider : EditorWindow
{
    private string folderPath = "";

    [MenuItem("Tools/Add BoxCollider to prefabs (first child object)")]
    public static void ShowWindow()
    {
        GetWindow<AddFurnitureCollider>("Add furniture colliders");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add BoxCollider to first child of prefabs", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Folder path", folderPath);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Colliders"))
        {
            ProcessPrefabs();
        }
    }

    private void ProcessPrefabs()
    {
        string[] prefabPaths = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);
        int count = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            if (prefab.transform.childCount == 0)
            {
                Debug.LogWarning($"{prefab.name} has no children, skipping..");
                continue;
            }

            Transform firstChild = prefab.transform.GetChild(0);

            // Check if theres already a collider, if yes skip
            if (firstChild.GetComponent<Collider>() != null)
            {
                Debug.Log($"{prefab.name} already has a collider, skipping..");
                continue;
            }

            Undo.RecordObject(firstChild.gameObject, "Add BoxCollider");

            BoxCollider collider = firstChild.gameObject.AddComponent<BoxCollider>();

            // Try resize based on mesh
            MeshRenderer mr = firstChild.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                collider.center = mr.localBounds.center;
                collider.size = mr.localBounds.size;
            }
            else
            {
                collider.center = Vector3.zero;
                collider.size = Vector3.one;
            }

            EditorUtility.SetDirty(prefab);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Added colliders to {count} prefabs in {folderPath}");
    }
}
