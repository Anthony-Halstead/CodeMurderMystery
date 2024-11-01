using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfEndAttribute : PropertyAttribute
{
    // Marks the end of the conditional visibility
}