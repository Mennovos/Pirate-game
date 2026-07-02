using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    
    [Space]
    [SerializeField] private List<AudioClip> hitSounds;
    [SerializeField] private AudioSource audioSource;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IEnemy enemy))
        {
            if (audioSource && hitSounds.Count > 0)
            {
                int index = Random.Range(0, hitSounds.Count);
                AudioClip clip = hitSounds[index];
        
                audioSource.PlayOneShot(clip);
            }
            
            playerHealth.TakeDamage(enemy.getAttackDamage());
        }
    }

    public void TakeDamage(float damage)
    {
        playerHealth.TakeDamage(damage);
    }
}
