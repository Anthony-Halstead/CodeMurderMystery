using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

public class TooltipEditorWindow : EditorWindow
{
    private Dictionary<Type, List<TooltipData>> tooltips = new Dictionary<Type, List<TooltipData>>();
    private Dictionary<Type, bool> foldoutStates = new Dictionary<Type, bool>(); // Store the foldout state for each class
    private Dictionary<Type, Vector2> classScrollPositions = new Dictionary<Type, Vector2>(); // Store scroll positions for each class foldout
    private Dictionary<Type, MonoScript> scriptCache = new Dictionary<Type, MonoScript>(); // Cache MonoScript references
    private Vector2 scrollPos;  // Main vertical scrolling position
    private string classFilter = "";  // Field to store the class name filter
    private bool showEmptyTooltipsOnly = false; // Field to store the empty tooltip filter
    private Type selectedClass = null; // Selected class for dropdown
    private string selectedNamespace = null; // Selected namespace for dropdown
    private string[] classNames; // Array of class names for the dropdown
    private Type[] classTypes; // Array of class types for the dropdown
    private string[] namespaceNames; // Array of namespace names for the dropdown

    private int itemsPerPage = 10; // Items per page
    private int currentPage = 1; // Current page
    private bool showAll = false; // Show all foldouts without pagination
    private bool hasCustomUnsavedChanges = false; // Track if there are unsaved changes

    private const int minItemsPerPage = 10; // Minimum items per page
    private const int maxVisibleTooltips = 5; // Maximum number of tooltips before enabling scrolling

    [MenuItem("Tools/Tooltip Editor")]
    public static void ShowWindow()
    {
        TooltipEditorWindow window = GetWindow<TooltipEditorWindow>("Tooltip Editor");
        window.RefreshTooltips();
    }

    private void OnEnable()
    {
        RefreshTooltips();
    }

    private void OnGUI()
    {
        // First row: Filter by class name, toggle for empty tooltips
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filter by Class Name (partial or full):", GUILayout.Width(200));
        string previousClassFilter = classFilter;
        classFilter = EditorGUILayout.TextField(classFilter);

        if (!string.IsNullOrEmpty(classFilter) && previousClassFilter != classFilter)
        {
            // If the class filter string is used, clear the selected class and namespace
            selectedClass = null;
            selectedNamespace = null;
        }

        showEmptyTooltipsOnly = EditorGUILayout.Toggle("Show Only Empty Tooltips", showEmptyTooltipsOnly, GUILayout.Width(200));

        EditorGUILayout.EndHorizontal();

        // Second row: Namespace and Class dropdowns
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Filter By Namespace:", GUILayout.Width(120));
        int selectedNamespaceIndex = string.IsNullOrEmpty(selectedNamespace) ? -1 : Array.IndexOf(namespaceNames, selectedNamespace);
        selectedNamespaceIndex = EditorGUILayout.Popup(selectedNamespaceIndex, namespaceNames, GUILayout.Width(200));

        if (selectedNamespaceIndex >= 0 && selectedNamespaceIndex < namespaceNames.Length)
        {
            selectedNamespace = namespaceNames[selectedNamespaceIndex];
            selectedClass = null; // Reset class filter when namespace is selected
            classFilter = ""; // Clear the text filter
        }

        EditorGUILayout.LabelField("Filter By Class:", GUILayout.Width(100));
        int selectedClassIndex = selectedClass == null ? -1 : Array.IndexOf(classTypes, selectedClass);
        selectedClassIndex = EditorGUILayout.Popup(selectedClassIndex, classNames, GUILayout.Width(200));

        if (selectedClassIndex >= 0 && selectedClassIndex < classTypes.Length)
        {
            selectedClass = classTypes[selectedClassIndex];
            selectedNamespace = null; // Reset namespace filter when class is selected
            classFilter = ""; // Clear the text filter
        }

        EditorGUILayout.EndHorizontal();

        // Third row: Items per page and "Show All" button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Items per Page:", GUILayout.Width(100));
        itemsPerPage = Mathf.Max(minItemsPerPage, EditorGUILayout.IntField(itemsPerPage, GUILayout.Width(50)));

        if (GUILayout.Button("Show All", GUILayout.Width(100)))
        {
            showAll = true;
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh Tooltips"))
        {
            RefreshTooltips();
        }

        // Filter the tooltips based on the current filters
        IEnumerable<KeyValuePair<Type, List<TooltipData>>> filteredTooltips = tooltips;

        if (!string.IsNullOrEmpty(classFilter))
        {
            filteredTooltips = filteredTooltips.Where(kvp => kvp.Key.Name.Contains(classFilter, StringComparison.OrdinalIgnoreCase));
        }
        else if (selectedClass != null)
        {
            filteredTooltips = filteredTooltips.Where(kvp => kvp.Key == selectedClass);
        }
        else if (!string.IsNullOrEmpty(selectedNamespace))
        {
            filteredTooltips = filteredTooltips.Where(kvp => kvp.Key.Namespace == selectedNamespace);
        }

        if (showEmptyTooltipsOnly)
        {
            filteredTooltips = filteredTooltips.Where(kvp => kvp.Value.Any(t => string.IsNullOrEmpty(t.NewTooltip)));
        }

        // Calculate pagination details based on filtered results
        int totalItems = filteredTooltips.Count();
        int totalPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);
        if (totalPages < 1) totalPages = 1;
        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        // Display pagination controls if not showing all items
        if (!showAll)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous", GUILayout.Width(100)) && currentPage > 1)
            {
                currentPage--;
            }

            EditorGUILayout.LabelField($"Page {currentPage} of {totalPages}", GUILayout.Width(150));

            if (GUILayout.Button("Next", GUILayout.Width(100)) && currentPage < totalPages)
            {
                currentPage++;
            }

            EditorGUILayout.EndHorizontal();

            // Paginate the filtered results
            filteredTooltips = filteredTooltips.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);
        }

        // Begin scrolling view
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Display the tooltips
        foreach (var classTooltips in filteredTooltips)
        {
            List<TooltipData> tooltipsToShow = classTooltips.Value;

            if (showEmptyTooltipsOnly)
            {
                tooltipsToShow = tooltipsToShow.Where(t => string.IsNullOrEmpty(t.NewTooltip)).ToList();
                if (tooltipsToShow.Count == 0)
                {
                    continue;
                }
            }

            if (!foldoutStates.ContainsKey(classTooltips.Key))
            {
                foldoutStates[classTooltips.Key] = false;
            }

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();
            foldoutStates[classTooltips.Key] = EditorGUILayout.Foldout(foldoutStates[classTooltips.Key], "Class: " + classTooltips.Key.Name, true, EditorStyles.foldoutHeader);

            MonoScript classScript = GetOrCacheMonoScript(classTooltips.Key);
            EditorGUILayout.ObjectField(classScript, typeof(MonoScript), false, GUILayout.MaxWidth(200));

            EditorGUILayout.EndHorizontal();

            if (foldoutStates[classTooltips.Key])
            {
                EditorGUI.indentLevel++;

                if (!classScrollPositions.ContainsKey(classTooltips.Key))
                {
                    classScrollPositions[classTooltips.Key] = Vector2.zero;
                }

                int tooltipCount = tooltipsToShow.Count;
                bool shouldScroll = tooltipCount > maxVisibleTooltips;
                float height = shouldScroll ? 5 * EditorGUIUtility.singleLineHeight : tooltipCount * EditorGUIUtility.singleLineHeight;

                classScrollPositions[classTooltips.Key] = EditorGUILayout.BeginScrollView(classScrollPositions[classTooltips.Key], GUILayout.Height(height), GUILayout.ExpandWidth(true));

                foreach (var tooltipData in tooltipsToShow)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                    GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Variable:", boldLabelStyle, GUILayout.Width(70));
                    EditorGUILayout.LabelField(tooltipData.VariableName, GUILayout.Width(150));

                    EditorGUILayout.LabelField("Tooltip:", boldLabelStyle, GUILayout.Width(70));
                    string newTooltip = EditorGUILayout.TextField(tooltipData.NewTooltip, GUILayout.ExpandWidth(true));

                    if (newTooltip != tooltipData.NewTooltip)
                    {
                        tooltipData.NewTooltip = newTooltip;
                        hasCustomUnsavedChanges = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Save Tooltips"))
        {
            SaveTooltips();
        }
    }

    private MonoScript GetOrCacheMonoScript(Type type)
    {
        if (!scriptCache.TryGetValue(type, out var monoScript))
        {
            monoScript = FindMonoScript(type);
            scriptCache[type] = monoScript;
        }
        return monoScript;
    }

    private MonoScript FindMonoScript(Type type)
    {
        MonoScript[] scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
        foreach (var script in scripts)
        {
            if (script.GetClass() == type)
            {
                return script;
            }
        }
        return null;
    }

    private void RefreshTooltips()
    {
        tooltips.Clear();
        foldoutStates.Clear();
        classScrollPositions.Clear();
        scriptCache.Clear();

        MonoScript[] scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
        List<Type> classesWithTooltips = new List<Type>();
        HashSet<string> namespaces = new HashSet<string>();

        foreach (MonoScript script in scripts)
        {
            Type scriptType = script.GetClass();
            if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                FieldInfo[] fields = scriptType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var field in fields)
                {
                    var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                    if (tooltipAttribute != null)
                    {
                        if (!tooltips.ContainsKey(scriptType))
                        {
                            tooltips[scriptType] = new List<TooltipData>();
                        }

                        tooltips[scriptType].Add(new TooltipData
                        {
                            VariableName = field.Name,
                            OriginalTooltip = tooltipAttribute.tooltip,
                            NewTooltip = tooltipAttribute.tooltip,
                            FieldInfo = field,
                            Script = script
                        });

                        if (!classesWithTooltips.Contains(scriptType))
                        {
                            classesWithTooltips.Add(scriptType);
                        }

                        if (!string.IsNullOrEmpty(scriptType.Namespace))
                        {
                            namespaces.Add(scriptType.Namespace);
                        }
                    }
                }
            }
        }

        classTypes = classesWithTooltips.ToArray();
        classNames = classTypes.Select(t => t.Name).ToArray();

        namespaceNames = namespaces.ToArray();

        currentPage = 1;
        showAll = false;
    }

    private void SaveTooltips()
    {
        foreach (var classTooltips in tooltips)
        {
            foreach (var tooltipData in classTooltips.Value)
            {
                if (tooltipData.NewTooltip != tooltipData.OriginalTooltip)
                {
                    UpdateScriptFile(tooltipData);
                }
            }
        }

        hasCustomUnsavedChanges = false;
        AssetDatabase.Refresh();
    }

    private void UpdateScriptFile(TooltipData tooltipData)
    {
        string scriptPath = AssetDatabase.GetAssetPath(tooltipData.Script);
        string[] lines = File.ReadAllLines(scriptPath);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains($"Tooltip(\"{tooltipData.OriginalTooltip}\""))
            {
                lines[i] = lines[i].Replace($"Tooltip(\"{tooltipData.OriginalTooltip}\"", $"Tooltip(\"{tooltipData.NewTooltip}\"");
                break;
            }
        }

        File.WriteAllLines(scriptPath, lines);
    }

    private void OnDestroy()
    {
        if (hasCustomUnsavedChanges)
        {
            int option = EditorUtility.DisplayDialogComplex(
                "Unsaved Changes",
                "You have unsaved changes. What would you like to do?",
                "Save",
                "Don't Save",
                "Cancel"
            );

            if (option == 0) // Save
            {
                SaveTooltips();
            }
            else if (option == 2) // Cancel
            {
                ShowWindow();
            }
        }
    }

    private class TooltipData
    {
        public string VariableName;
        public string OriginalTooltip;
        public string NewTooltip;
        public FieldInfo FieldInfo;
        public MonoScript Script;
    }
}
