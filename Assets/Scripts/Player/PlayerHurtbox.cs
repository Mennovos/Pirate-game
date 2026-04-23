using System;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    [SerializeField] private Health playerHealth;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IEnemy enemy))
        {
            playerHealth.TakeDamage(enemy.getAttackDamage());
        }
    }
}
