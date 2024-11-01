using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericParticleLibrary", menuName = "Systems/Libraries/GenericParticleLibrary")]
public class GenericParticleLibrary : ScriptableObject
{
    [SerializeField] protected List<ParticleData> data = new();
    public List<ParticleData> Data => data;
}
