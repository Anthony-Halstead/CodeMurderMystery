using UnityEditor;
using UnityEngine;
using System;
[CustomEditor(typeof(MonoBehaviour), true)]
public class ConditionalEditor : Editor
{
    private bool groupFoldout = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var iterator = serializedObject.GetIterator();
        iterator.NextVisible(true); // Skip the script field

        while (iterator.NextVisible(false))
        {
            bool shouldShow = EvaluateConditions(iterator);

            // Handle ShowIfGroup and foldout for grouping fields
            if (iterator.propertyType == SerializedPropertyType.Generic && ShouldEnterGroup(iterator, out ShowIfGroupAttribute groupAttribute))
            {
                groupFoldout = EditorGUILayout.Foldout(groupFoldout, ObjectNames.NicifyVariableName(groupAttribute.ConditionFieldName));

                if (groupFoldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(iterator, true);
                    DisplayGroupFields(iterator);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    SkipGroup(iterator);
                }
            }
            else if (shouldShow)
            {
                // Display regular fields based on ShowIf/HideIf
                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool EvaluateConditions(SerializedProperty property)
    {
        bool shouldShow = true;

        var showIf = GetAttribute<ShowIfAttribute>(property);
        var hideIf = GetAttribute<HideIfAttribute>(property);

        if (showIf != null) shouldShow &= CheckCondition(property, showIf.ConditionFieldName, showIf.DesiredValue);
        if (hideIf != null) shouldShow &= !CheckCondition(property, hideIf.ConditionFieldName, hideIf.DesiredValue);

        return shouldShow;
    }

    private bool CheckCondition(SerializedProperty property, string conditionFieldName, object desiredValue)
    {
        var conditionProperty = property.serializedObject.FindProperty(conditionFieldName);

        if (conditionProperty == null)
        {
            Debug.LogWarning($"Condition field '{conditionFieldName}' not found.");
            return true;
        }

        // Handle boolean, enum, and integer conditions
        if (conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProperty.boolValue.Equals(desiredValue ?? true);
        }
        if ((conditionProperty.propertyType == SerializedPropertyType.Enum || conditionProperty.propertyType == SerializedPropertyType.Integer) && desiredValue != null)
        {
            int enumValue = (int)desiredValue;
            return conditionProperty.enumValueIndex == enumValue || conditionProperty.intValue == enumValue;
        }

        return false;
    }

    private T GetAttribute<T>(SerializedProperty property) where T : PropertyAttribute
    {
        var field = property.serializedObject.targetObject.GetType().GetField(property.name);
        return field != null ? Attribute.GetCustomAttribute(field, typeof(T)) as T : null;
    }

    private bool ShouldEnterGroup(SerializedProperty property, out ShowIfGroupAttribute groupAttribute)
    {
        groupAttribute = GetAttribute<ShowIfGroupAttribute>(property);
        return groupAttribute != null && CheckCondition(property, groupAttribute.ConditionFieldName, groupAttribute.DesiredValue);
    }

    private void DisplayGroupFields(SerializedProperty iterator)
    {
        while (iterator.NextVisible(false))
        {
            if (iterator.propertyType == SerializedPropertyType.Generic && GetAttribute<ShowIfGroupEndAttribute>(iterator) != null)
            {
                break;
            }
            EditorGUILayout.PropertyField(iterator, true);
        }
    }

    private void SkipGroup(SerializedProperty iterator)
    {
        int groupDepth = iterator.depth;
        while (iterator.NextVisible(false) && iterator.depth > groupDepth) { }
    }
}
