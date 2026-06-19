using System.Collections.Generic;
using UnityEngine;

public class GroundSoundSupplier : MonoBehaviour
{
    [SerializeField] private List<AudioClip> walkSounds;
    
    public List<AudioClip> WalkSounds => walkSounds;
}
