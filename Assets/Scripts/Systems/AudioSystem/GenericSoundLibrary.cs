
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GenericSoundLibrary", menuName = "Systems/Libraries/GenericSoundLibrary")]
public class GenericSoundLibrary : ScriptableObject
{
    [SerializeField] protected List<SoundData> data = new();
    public List<SoundData> Data => data;
}
