using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class HideIfAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; }
    public object DesiredValue { get; }

    public HideIfAttribute(string conditionFieldName, object desiredValue = null)
    {
        ConditionFieldName = conditionFieldName;
        DesiredValue = desiredValue;
    }
}