using System;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfGroupAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; }
    public object DesiredValue { get; }
    public bool Foldout { get; }

    public ShowIfGroupAttribute(string conditionFieldName, object desiredValue = null, bool foldout = false)
    {
        ConditionFieldName = conditionFieldName;
        DesiredValue = desiredValue;
        Foldout = foldout;
    }
}