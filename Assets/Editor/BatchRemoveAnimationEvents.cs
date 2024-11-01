using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchRemoveAnimationEvents : EditorWindow
{
    [MenuItem("Tools/Batch Remove Animation Events")]
    static void Init()
    {
        // Initialize the editor window
        BatchRemoveAnimationEvents window = (BatchRemoveAnimationEvents)EditorWindow.GetWindow(typeof(BatchRemoveAnimationEvents));
        window.titleContent = new GUIContent("Remove Animation Events");
        window.Show();
    }

    // List of animation packages to process
    public List<Object> animationPackages = new List<Object>();

    // Scroll position for the window
    private Vector2 scrollPosition;

    void OnGUI()
    {
        // Title
        EditorGUILayout.LabelField("Batch Remove Animation Events", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Create a serialized object for this editor window
        SerializedObject so = new SerializedObject(this);
        SerializedProperty packagesProperty = so.FindProperty("animationPackages");

        // Begin scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Display the animation packages list with the ability to drag and drop multiple assets
        EditorGUILayout.PropertyField(packagesProperty, new GUIContent("Animation Packages"), true);
        so.ApplyModifiedProperties();

        EditorGUILayout.Space();

        // 'Remove Animation Events' button
        if (GUILayout.Button("Remove Animation Events"))
        {
            RemoveAnimationEvents();
        }

        // End scroll view
        EditorGUILayout.EndScrollView();
    }

    void RemoveAnimationEvents()
    {
        if (animationPackages == null || animationPackages.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please add animation packages to the list.", "OK");
            return;
        }

        int totalClips = 0;
        int totalEventsRemoved = 0;

        foreach (Object package in animationPackages)
        {
            if (package == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(package);

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Could not get asset path for " + package.name);
                continue;
            }

            // Load all assets at the path
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in assets)
            {
                if (asset is AnimationClip)
                {
                    AnimationClip clip = asset as AnimationClip;

                    // Skip Unity's built-in '__preview__' clips
                    if (clip.name.Contains("__preview__"))
                        continue;

                    totalClips++;

                    // Explicitly use UnityEngine.AnimationEvent[]
                    UnityEngine.AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);

                    if (events != null && events.Length > 0)
                    {
                        // Remove all events
                        AnimationUtility.SetAnimationEvents(clip, new UnityEngine.AnimationEvent[0]);
                        Debug.Log($"Removed {events.Length} events from clip: {clip.name}");
                        totalEventsRemoved += events.Length;

                        // Mark the clip as dirty so that changes are saved
                        EditorUtility.SetDirty(clip);
                    }
                }
            }
        }

        // Save all changes and refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Completed", $"Processed {totalClips} clips and removed {totalEventsRemoved} events.", "OK");
    }
}
