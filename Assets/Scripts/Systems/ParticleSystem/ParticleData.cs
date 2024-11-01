using System;
using UnityEngine;

[Serializable]
public class ParticleData
{
    [Tooltip("The particle that will be played.")]
    public ParticleSystem vfx;
    public bool frequentParticle;
    public Vector3 particlePositionOffset;
    public Vector3 particleRotationOffset;
}
