using System.Collections.Generic;
using UnityEngine;

public abstract class EffectHandler : MonoBehaviour
{
    [Tooltip("List of PhysicMaterials to ignore when detecting.")]
    [SerializeField] protected List<PhysicMaterial> ignoredMaterials;
    [Min(0), SerializeField] protected float materialCheckDistance = 2.0f; // Adjusted distance as needed
}
