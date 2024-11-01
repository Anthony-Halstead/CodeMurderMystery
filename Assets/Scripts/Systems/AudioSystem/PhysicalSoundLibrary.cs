using UnityEngine;
[CreateAssetMenu(fileName = "PhysicalSoundLibrary", menuName = "Systems/Libraries/PhysicalSoundLibrary")]
public class PhysicalSoundLibrary : GenericSoundLibrary
{
    [SerializeField] PhysicMaterial materialToMatch;
    public PhysicMaterial MaterialToMatch => materialToMatch;
}
