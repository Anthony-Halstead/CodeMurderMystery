using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class ShowIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool shouldShow = EvaluateCondition(property, out ShowIfAttribute showIf, out HideIfAttribute hideIf);

        if (hideIf != null) shouldShow = !shouldShow; // invert visibility for HideIf

        if (showIf != null && showIf.DisableInsteadOfHide && !shouldShow)
        {
            GUI.enabled = false;
        }

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        if (showIf != null && showIf.DisableInsteadOfHide && !shouldShow)
        {
            GUI.enabled = true; // Reset GUI.enabled after rendering
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        bool shouldShow = EvaluateCondition(property, out ShowIfAttribute showIf, out HideIfAttribute hideIf);
        return (shouldShow || (showIf != null && showIf.DisableInsteadOfHide)) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
    }

    private bool EvaluateCondition(SerializedProperty property, out ShowIfAttribute showIf, out HideIfAttribute hideIf)
    {
        showIf = attribute as ShowIfAttribute;
        hideIf = attribute as HideIfAttribute;
        string conditionFieldName = showIf?.ConditionFieldName ?? hideIf?.ConditionFieldName;
        object desiredValue = showIf?.DesiredValue ?? hideIf?.DesiredValue;

        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionFieldName);

        if (conditionProperty == null)
        {
            Debug.LogWarning($"Condition field '{conditionFieldName}' not found.");
            return true;
        }

        // Check boolean condition
        if (conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProperty.boolValue.Equals(desiredValue ?? true);
        }

        // Check enum or integer condition
        if ((conditionProperty.propertyType == SerializedPropertyType.Enum || conditionProperty.propertyType == SerializedPropertyType.Integer) && desiredValue != null)
        {
            int enumValue = (int)desiredValue;
            return conditionProperty.enumValueIndex == enumValue || conditionProperty.intValue == enumValue;
        }

        Debug.LogWarning($"Unsupported property type for condition: {conditionProperty.propertyType}");
        return true;
    }
}
