using UnityEngine;
[CreateAssetMenu(fileName = "PhysicalParticleLibrary", menuName = "Systems/Libraries/PhysicalParticleLibrary")]
public class PhysicalParticleLibrary : GenericParticleLibrary
{
    [SerializeField] PhysicMaterial materialToMatch;
    public PhysicMaterial MaterialToMatch => materialToMatch;
}
