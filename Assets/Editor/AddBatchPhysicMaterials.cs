using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddBatchPhysicMaterials : EditorWindow
{
    [MenuItem("Tools/AddBatchPhysicMaterials")]
    static void Init()
    {
        AddBatchPhysicMaterials window = (AddBatchPhysicMaterials)EditorWindow.GetWindow(typeof(AddBatchPhysicMaterials));
        window.Show();
    }

    // List of prefabs to process
    public List<GameObject> prefabs = new List<GameObject>();

    // List containing information about each prefab's colliders and selection state
    public List<PrefabColliderInfo> prefabColliderInfos = new List<PrefabColliderInfo>();

    // The physics material to apply
    public PhysicMaterial physicsMaterial;

    // Scroll position for the window
    private Vector2 scrollPosition;

    // Called when the window is enabled
    void OnEnable()
    {
        // Initialize the collider info list
        UpdatePrefabColliderInfos();
    }

    void OnGUI()
    {
        // Create a serialized object for this editor window
        SerializedObject so = new SerializedObject(this);
        SerializedProperty prefabsProperty = so.FindProperty("prefabs");

        // Begin scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Display the prefab list with the ability to drag and drop multiple prefabs
        EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("Prefabs"), true);
        so.ApplyModifiedProperties();

        // Physics material field
        physicsMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", physicsMaterial, typeof(PhysicMaterial), false);

        // Update the collider info list if the prefab list has changed
        UpdatePrefabColliderInfos();

        // Display colliders and selection options for each prefab
        foreach (PrefabColliderInfo info in prefabColliderInfos)
        {
            if (info.prefab == null) continue;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(info.prefab.name, EditorStyles.boldLabel);

            if (info.colliders == null || info.colliders.Length == 0)
            {
                EditorGUILayout.LabelField("No colliders found.");
                continue;
            }

            // Use a foldout to toggle the display of colliders
            info.showColliders = EditorGUILayout.Foldout(info.showColliders, "Colliders");

            if (info.showColliders)
            {
                EditorGUI.indentLevel++;
                if (info.colliders.Length > 1)
                {
                    // Display checkboxes for each collider if there are multiple
                    for (int i = 0; i < info.colliders.Length; i++)
                    {
                        info.selected[i] = EditorGUILayout.ToggleLeft(
                            $"{info.colliders[i].GetType().Name} ({info.colliders[i].name})",
                            info.selected[i]);
                    }
                }
                else
                {
                    // Inform that there is only one collider
                    EditorGUILayout.LabelField($"Single Collider: {info.colliders[0].GetType().Name} ({info.colliders[0].name})");
                }
                EditorGUI.indentLevel--;
            }
        }

        // End scroll view
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 'Apply' button to assign the physics material
        if (GUILayout.Button("Apply"))
        {
            ApplyPhysicsMaterial();
        }
    }

    // Updates the collider info list when the prefab list changes
    void UpdatePrefabColliderInfos()
    {
        // Check if the prefabColliderInfos needs to be updated
        if (prefabColliderInfos.Count != prefabs.Count)
        {
            RebuildPrefabColliderInfos();
            return;
        }

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] != prefabColliderInfos[i].prefab)
            {
                RebuildPrefabColliderInfos();
                return;
            }
        }
    }

    // Rebuilds the collider info list
    void RebuildPrefabColliderInfos()
    {
        prefabColliderInfos.Clear();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;

            PrefabColliderInfo info = new PrefabColliderInfo();
            info.prefab = prefab;

            // Get all colliders in the prefab
            Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
            info.colliders = colliders;
            info.selected = new bool[colliders.Length];

            // Initialize selection states to false
            for (int i = 0; i < colliders.Length; i++)
            {
                info.selected[i] = false;
            }

            // Initialize foldout state
            info.showColliders = false;

            prefabColliderInfos.Add(info);
        }
    }

    // Applies the physics material to the selected colliders
    void ApplyPhysicsMaterial()
    {
        if (physicsMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Physics Material.", "OK");
            return;
        }

        foreach (PrefabColliderInfo info in prefabColliderInfos)
        {
            if (info.prefab == null) continue;

            // Get the path of the prefab asset
            string prefabPath = AssetDatabase.GetAssetPath(info.prefab);

            // Load the prefab contents for editing
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            // Get all colliders in the prefab instance
            Collider[] colliders = prefabInstance.GetComponentsInChildren<Collider>(true);

            if (colliders.Length == 0)
            {
                // No colliders found; skip this prefab
                PrefabUtility.UnloadPrefabContents(prefabInstance);
                continue;
            }
            else if (colliders.Length == 1)
            {
                // Only one collider; apply the physics material
                colliders[0].sharedMaterial = physicsMaterial;
            }
            else
            {
                // Multiple colliders
                bool anySelected = false;
                foreach (bool selected in info.selected)
                {
                    if (selected)
                    {
                        anySelected = true;
                        break;
                    }
                }

                if (anySelected)
                {
                    // Apply to selected colliders
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (info.selected[i])
                        {
                            colliders[i].sharedMaterial = physicsMaterial;
                        }
                    }
                }
                else
                {
                    // No colliders selected; apply to the first collider
                    colliders[0].sharedMaterial = physicsMaterial;
                }
            }

            // Save the modified prefab instance
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        // Save all changes and refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Physics Material applied successfully.", "OK");
    }

    // Serializable class to store collider info for each prefab
    [System.Serializable]
    public class PrefabColliderInfo
    {
        public GameObject prefab;
        public Collider[] colliders;
        public bool[] selected;

        // Foldout state for displaying colliders
        public bool showColliders = false;
    }
}
