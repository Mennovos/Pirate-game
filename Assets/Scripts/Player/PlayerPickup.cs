using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private List<AudioClip> grabSounds;
    [SerializeField] private AudioSource audioSource;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IPickup pickup))
        {
            if (audioSource && grabSounds.Count > 0)
            {
                int index = Random.Range(0, grabSounds.Count);
                AudioClip clip = grabSounds[index];
        
                audioSource.PlayOneShot(clip);
            }
            
            pickup.onGrab(gameObject);
        }
    }
}
