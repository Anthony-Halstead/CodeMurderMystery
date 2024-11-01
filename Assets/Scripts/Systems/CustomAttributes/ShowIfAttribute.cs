using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; }
    public object DesiredValue { get; }
    public bool DisableInsteadOfHide { get; }

    public ShowIfAttribute(string conditionFieldName, object desiredValue = null, bool disableInsteadOfHide = false)
    {
        ConditionFieldName = conditionFieldName;
        DesiredValue = desiredValue;
        DisableInsteadOfHide = disableInsteadOfHide;
    }
}

