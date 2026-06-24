using System;
using UnityEngine;

public class Deathwater : MonoBehaviour
{
    [SerializeField] private float damage = 2.5f;
    [SerializeField] private Vector2 knockback = new(0f, 10f);
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);

            if (other.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = knockback;
            }
        }

        if (other.TryGetComponent(out IEnemy enemy))
        {
            Debug.Log("killed");
            enemy.kill();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            if (other.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = knockback;
            }
        }
    }
}
