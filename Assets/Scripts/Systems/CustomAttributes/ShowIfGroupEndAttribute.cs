using System;

using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfGroupEndAttribute : PropertyAttribute
{
    // Defines the end of a conditional display group
}