using System;
using UnityEngine;

[Serializable]
public class ParticleData
{
    [Tooltip("The particle prefab that will be instantiated and played.")]
    public GameObject vfxPrefab;
    public bool frequentParticle;
    public Vector3 particlePositionOffset;
    public Vector3 particleRotationOffset;
}
